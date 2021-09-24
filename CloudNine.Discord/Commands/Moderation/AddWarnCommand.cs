using System.Linq;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;

using CloudNine.Core.Database;
using CloudNine.Core.Moderation;

using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.Exceptions;

using static CloudNine.Core.Moderation.ModCore;
using System;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using DSharpPlus.SlashCommands;
using DSharpPlus.SlashCommands.Attributes;

namespace CloudNine.Discord.Commands.Moderation
{
    public partial class ModerationCommands : SlashCommandBase
    {
        public partial class WarnCommands : SlashCommandBase
        {
            [SlashCommand("add", "Warns a user against an action.")]
            [SlashRequireUserPermissions(Permissions.ManageMessages)]
            [SlashRequireGuild]
            public async Task AddWarnCommandAsync(InteractionContext ctx,
                [Option("User", "Member to log a warning for.")]
                DiscordUser toWarnUser,

                [Option("Message", "Warn message to record.")]
                string warnMessage,
                
                [Option("Notify", "Notify the user? (sends the warn message).")]
                bool notify = false)
            {
                await ctx.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource);

                var toWarn = await ctx.Guild.GetMemberAsync(toWarnUser.Id);

                string msg = warnMessage;

                if (msg == "")
                    msg = "Default Warn.";

                var _database = _services.GetRequiredService<CloudNineDatabaseModel>();
                var mod = await _database.FindAsync<ModCore>(ctx.Guild.Id);

                if (mod is null)
                {
                    mod = new ModCore(ctx.Guild.Id);
                    await _database.AddAsync(mod);
                    await _database.SaveChangesAsync();
                }

                try
                {
                    if (mod.AddWarn(toWarn.Id, msg, ctx.User.Id, out var warn))
                    {
                        _database.Update(mod);
                        await _database.SaveChangesAsync();

                        if (notify)
                        {
                            var dm = await toWarn.CreateDmChannelAsync();
                            await dm.SendMessageAsync($"{msg}\n\n" +
                                $"*This is an automated message. Do not reply to this bot, your message will not been seen. " +
                                $"Please send questions to {ctx.User.Mention}*");
                        }

                        await RespondWarn($"Successfuly logged warning for user {toWarn.Mention} as `{warn.Key}`. {(notify ? "The user has been notified." : "")}");

                        if (mod.Warns.TryGetValue(toWarn.Id, out var warnings))
                        {
                            if (mod.ModlogNotices.TryGetValue(warnings.Where(x => !x.Value.Forgiven).Count(), out var notice))
                            {
                                await DisplayWarnNotice(ctx, notice, toWarn);
                            }
                        }
                    }
                    else
                    {
                        await RespondError("Failed to create a new warn or this warn has already been added.\n\n" +
                            "(This error should not be show, please notfiy a developer if it does appear)");
                    }
                }
                catch (InvalidUniqueKeyException ex)
                {
                    ctx.Client.Logger.LogError(ex, "Failed to generate a unique key.");
                    await RespondError("Failed to generate a unique warn key. Please try this command again.");
                }
            }

            public static async Task DisplayWarnNotice(InteractionContext ctx, string notice, DiscordMember memberFor)
            {
                var embed = ModBase()
                    .WithTitle($"Modlog Notice for {memberFor.Username}#{memberFor.Discriminator} ({memberFor.DisplayName})")
                    .WithDescription(notice);

                await ctx.FollowUpAsync(new DiscordFollowupMessageBuilder()
                    .WithContent(ctx.Member.Mention)
                    .AddEmbed(embed));
            }

            [SlashCommand("view", "View an exsisting warn.")]
            [SlashRequireUserPermissions(Permissions.ManageMessages)]
            public async Task ViewWarnCommand(InteractionContext ctx,
                [Option("ID", "Warn to view")]
                string warnId)
            {
                await ctx.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource);

                var _database = _services.GetRequiredService<CloudNineDatabaseModel>();
                var mod = await _database.FindAsync<ModCore>(ctx.Guild.Id);

                if (mod is null)
                {
                    await RespondError("There are no warnings on this server!");
                    return;
                }

                var warn = mod.WarnSet.FirstOrDefault(x => x.Key == warnId);

                if (warn is not null)
                {
                    string username;
                    try
                    {
                        var m = await ctx.Guild.GetMemberAsync(warn.SavedBy);
                        if (m is not null)
                            username = m.DisplayName;
                        else
                            username = warn.SavedBy.ToString();
                    }
                    catch (NotFoundException)
                    {
                        username = warn.SavedBy.ToString();
                    }

                    var embed = new DiscordEmbedBuilder()
                    .WithColor(Color_Warn)
                    .WithTitle($"Warn: `{warn.Key}`")
                    .WithAuthor($"User: {warn.UserId}", null, null)
                    .WithFooter($"Saved By: {username}")
                    .WithTimestamp(warn.CreatedOn);

                    if (warn.Forgiven)
                        embed.WithDescription("**This warn has been marked as FORGIVEN**");

                    if (warn.Reverts.Count > 0)
                    {
                        string data = "";
                        foreach (var w in warn.Reverts.Keys)
                            data += $"{w:g}";

                        embed.AddField($"*`{warn.Reverts.Count} reverts found:`*", $"`{data}`");
                    }

                    embed.AddField($"Last Edit: `{warn.LastEdit:g}`", $"```{(warn.Message == "" ? "Default warning." : warn.Message)}```");

                    if (warn.Edits.Count > 0)
                    {
                        List<string> data = new List<string>();
                        foreach (var w in warn.Edits.Keys)
                            data.Add($"{w:g}");

                        embed.AddField($"*`{warn.Edits.Count} edits found:`*", $"`{string.Join("`, `", data)}`");
                    }

                    await RespondAsync(embed: embed);
                }
                else
                {
                    await RespondError("Warn not found.");
                }
            }
        }
    }
}
