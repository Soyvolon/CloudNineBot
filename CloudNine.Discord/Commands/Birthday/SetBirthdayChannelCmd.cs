using System.Threading.Tasks;

using CloudNine.Discord.Utilities;

using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;

namespace CloudNine.Discord.Commands.Birthday
{
    public class SetBirthdayChannelCmd : CommandModule
    {
        private readonly BirthdayManager _birthdays;

        public SetBirthdayChannelCmd(BirthdayManager birthdays)
        {
            _birthdays = birthdays;
        }

        [Command("bdaychannel")]
        [RequireGuild]
        [Description("Sets the birthday channel for this server.")]
        [RequireUserPermissions(Permissions.ManageGuild)]
        public async Task SetBirthdayChannelAsync(CommandContext ctx, DiscordChannel channel)
        {
            _birthdays.UpdateBirthdayChannel(ctx.Guild.Id, channel);
            await ctx.RespondAsync($"Set the birthday channel to {channel.Mention}.").ConfigureAwait(false);
        }
    }
}
