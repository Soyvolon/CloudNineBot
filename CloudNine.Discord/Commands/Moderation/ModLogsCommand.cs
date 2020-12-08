using System.Threading.Tasks;

using CloudNine.Core.Database;
using CloudNine.Core.Moderation;

using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;

namespace CloudNine.Discord.Commands.Moderation
{
    public class ModLogsCommand : CommandModule
    {
        private readonly CloudNineDatabaseModel _database;

        public ModLogsCommand(CloudNineDatabaseModel database)
        {
            _database = database;
        }

        [Command("modlogs")]
        [Priority(2)]
        [Description("Gets the Mod Logs for a user.")]
        [Aliases("mlogs")]
        [RequireUserPermissions(Permissions.AccessChannels)]
        public async Task ModLogsCommandAsync(CommandContext ctx,
            [Description("Member to get logs for")]
            DiscordMember member)
        {
            var mod = await _database.FindAsync<ModCore>(ctx.Guild.Id);

            if(mod is null)
            {
                await RespondError("There are no warnings on this server!");
                return;
            }

            if (mod.Warns.TryGetValue(member.Id, out var warns))
            {
                var embed = new DiscordEmbedBuilder()
                    .WithColor(DiscordColor.Blurple)
                    .WithTitle($"ID: {member.Id}")
                    .WithAuthor($"Modlogs For: {member.DisplayName}", null, member.AvatarUrl)
                    .WithFooter($"Total Warns: {warns.Count}", null);

                int c = 0;
                foreach (var warn in warns.Values)
                {
                    embed.AddField($"Warn: `{warn.Key}`", 
                        $"Created On: `{warn.CreatedOn:g}` - Last Edit: `{warn.LastEdit:g}`\n" +
                        $"{(warn.Reverts.Count > 0 ? $"*`reverted {warn.Reverts.Count} times ...`*" : "")}\n" +
                        $"```\n" +
                        $"{warn.Message}" +
                        $"```\n" +
                        $"{(warn.Edits.Count > 0 ? $"*`... edited {warn.Edits.Count} times`*" : "")}");

                    if(c++ >= 4)
                    {
                        embed.AddField("", $"*`... {warns.Count - 4} older warns not dispalyed`*");
                        break;
                    }
                }

                await ctx.RespondAsync(embed: embed);
            }
            else
            {
                await RespondWarn("No warning found for user.");
            }
        }

        [Command("modlogs")]
        public async Task ShowLatestModLogsAsync(CommandContext ctx)
        {
            var mod = await _database.FindAsync<ModCore>(ctx.Guild.Id);

            if (mod is null)
            {
                await RespondError("There are no warnings on this server!");
                return;
            }


        }
    }
}
