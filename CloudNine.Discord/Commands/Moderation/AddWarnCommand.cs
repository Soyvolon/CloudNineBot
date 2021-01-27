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

namespace CloudNine.Discord.Commands.Moderation
{
    public class AddWarnCommand : CommandModule
    {
        private readonly IServiceProvider _services;

        public AddWarnCommand(IServiceProvider services)
        {
            this._services = services;
        }

        [Command("warn")]
        [Priority(2)]
        [Description("Warns a user against an action. Use `--notify` to send the warn message to the user.")]
        [RequireUserPermissions(Permissions.ManageMessages)]
        public async Task AddWarnCommandAsync(CommandContext ctx,
            [Description("Member to log a warning for.")]
            DiscordMember? toWarn = null,

            [Description("Warn message to record.")]
            [RemainingText]
            string warnMessage = "")
        {
            if(toWarn is null)
            {
                await RespondError("Failed to get a user to warn. Make sure the user is on the server and the ID is correct, or use a mention.");
                return;
            }

            string msg;
            bool notify = false;
            if(warnMessage.Contains("--notify"))
            {
                if(toWarn.IsBot)
                {
                    await RespondError("Cannot notify bot users.");
                    return;
                }

                notify = true;
                msg = warnMessage.Replace("--notify", string.Empty);
            }
            else
            {
                msg = warnMessage;
            }

            if (msg == "")
                msg = "Default Warn.";

            var _database = _services.GetRequiredService<CloudNineDatabaseModel>();
            var mod = await _database.FindAsync<ModCore>(ctx.Guild.Id);

            if(mod is null)
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

                    if(mod.Warns.TryGetValue(toWarn.Id, out var warnings))
                    {
                        if(mod.ModlogNotices.TryGetValue(warnings.Where(x => !x.Value.Forgiven).Count(), out var notice))
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

        [Command("warn")]
        [Priority(1)]
        public async Task AddWarnCommandAsync(CommandContext ctx,
            [Description("User ID to warn")]
            ulong userId,

            [Description("Message to record")]
            string warnMessage = "")
        {
            try
            {
                await AddWarnCommandAsync(ctx, await ctx.Guild.GetMemberAsync(userId), warnMessage);
            }
            catch(NotFoundException)
            {
                await RespondError("Failed to get a user to warn. Make sure the user is on the server and the ID is correct, or use a mention.");
            }
        }

        public static async Task DisplayWarnNotice(CommandContext ctx, string notice, DiscordMember memberFor)
        {
            var embed = ModBase()
                .WithTitle($"Modlog Notice for {memberFor.Username}#{memberFor.Discriminator} ({memberFor.DisplayName})")
                .WithDescription(notice);

            await ctx.RespondAsync(content: ctx.Member.Mention, embed: embed);
        }

        [Command("warn")]
        [Priority(0)]
        public async Task ViewWarnCommand(CommandContext ctx,
            [Description("Warn to view")]
            string warnId)
        {
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

                await ctx.RespondAsync(embed: embed);
            }
            else
            {
                await RespondError("Warn not found.");
            }
        }
    }
}
