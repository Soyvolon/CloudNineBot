﻿using System.Threading.Tasks;

using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;

namespace CloudNine.Discord.Commands.Birthday
{
    public class SetBirthdayChannelCmd : CommandModule
    {
        [Command("bdaychannel")]
        [RequireGuild]
        [Description("Sets the birthday channel for this server.")]
        [RequireUserPermissions(Permissions.ManageGuild)]
        public async Task SetBirthdayChannelAsync(CommandContext ctx, DiscordChannel channel)
        {
            DiscordBot.Bot.Birthdays.UpdateBirthdayChannel(ctx.Guild.Id, channel);
            await ctx.RespondAsync($"Set the birthday channel to {channel.Mention}.").ConfigureAwait(false);
        }
    }
}
