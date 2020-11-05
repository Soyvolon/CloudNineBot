using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;

namespace CloudNine.Discord.Commands.Birthday
{
    public class ForceChannelDescUpdateCmd : CommandModule
    {
        [Command("forceupdate")]
        [Description("Forces the birthday channel description to update.")]
        [RequireUserPermissions(DSharpPlus.Permissions.Administrator)]
        public async Task ForceChannelDescUpdateAsync(CommandContext ctx)
        {
            if (await DiscordBot.Bot.Birthdays.ForceChannelUpdate(ctx.Guild))
                await ctx.RespondAsync("Channel updated.");
            else await ctx.RespondAsync("Failed to update channel. Make sure a birthday channel is set.");
        }
    }
}