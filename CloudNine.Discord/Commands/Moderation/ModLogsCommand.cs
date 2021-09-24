using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using CloudNine.Core.Database;
using CloudNine.Core.Moderation;

using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.Exceptions;
using DSharpPlus.SlashCommands;
using DSharpPlus.SlashCommands.Attributes;

using Microsoft.Extensions.DependencyInjection;

namespace CloudNine.Discord.Commands.Moderation
{
    public partial class ModerationCommands : SlashCommandBase
    {
        [SlashCommandGroup("logs", "Log module.")]
        public partial class LogCommands : SlashCommandBase
        {
            private readonly IServiceProvider _services;

            public LogCommands(IServiceProvider services)
            {
                _services = services;
            }

            [SlashCommand("view", "Gets the Mod Logs for a user.")]
            [SlashRequireUserPermissions(Permissions.ManageMessages)]
            public async Task ModLogsCommandAsync(InteractionContext ctx,
                [Option("User", "Member to get logs for")]
                DiscordUser member)
            {
                await ctx.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource);

                var _database = _services.GetRequiredService<CloudNineDatabaseModel>();
                var mod = await _database.FindAsync<ModCore>(ctx.Guild.Id);

                if (mod is null)
                {
                    await RespondError("There are no warnings on this server!");
                    return;
                }

                if (mod.Warns.TryGetValue(member.Id, out var warns))
                {
                    var validWarns = warns.Where(x => !x.Value.Forgiven).Count();
                    var embed = new DiscordEmbedBuilder()
                        .WithColor(DiscordColor.Blurple)
                        .WithTitle($"ID: {member.Id}")
                        .WithAuthor($"Modlogs For: {member.Username}", null, member.AvatarUrl)
                        .WithFooter($"Total Valid Warns: {validWarns} | Forgiven Warns: {warns.Count - validWarns}", null);

                    int c = 0;
                    List<string>? extraWarns = null;
                    foreach (var warn in warns.Values)
                    {
                        if (c++ >= 10)
                        {
                            if (extraWarns is null) extraWarns = new();

                            extraWarns.Add(warn.Key);
                        }
                        else
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

                            embed.AddField($"Warn: `{warn.Key}`{(warn.Forgiven ? " - FORGIVEN" : "")}",
                                $"Created On: `{warn.CreatedOn:g}` - Last Edit: `{warn.LastEdit:g}`\n" +
                                $"{(warn.Reverts.Count > 0 ? $"*`reverted {warn.Reverts.Count} times ...`*" : "")}\n" +
                                $"```\n" +
                                $"{warn.Message}" +
                                $"```\n" +
                                $"{(warn.Edits.Count > 0 ? $"*`... edited {warn.Edits.Count} times`*" : "")}\n" +
                                $"Warn created by: {username}");
                        }
                    }

                    if (extraWarns is not null)
                    {
                        var joined = string.Join("`, `", extraWarns);

                        embed.AddField($"*`... {warns.Count - 10} older warns not dispalyed`*",
                            $"`{joined}`");
                    }

                    await RespondAsync(embed);
                }
                else
                {
                    await RespondWarn("No warning found for user.");
                }
            }
        }
    }
}
