using System.Threading.Tasks;

using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;

namespace CloudNine.Discord.Commands.Birthday
{
    public class SetBirthdayChannelCmd : BaseCommandModule
    {
        [Command("channel")]
        [Description("Sets the birthday channel for this server.")]
        [RequireUserPermissions(Permissions.Administrator)]
        public async Task SetBirthdayChannelAsync(CommandContext ctx, DiscordChannel channel)
        {
            DiscordBot.Bot.Birthdays.UpdateBirthdayChannel(ctx.Guild.Id, channel);
            await ctx.RespondAsync($"Set the birthday channel to {channel.Mention}. Unless otherwise set, the deafult lockout time is 30 days.").ConfigureAwait(false);
        }
    }
}
