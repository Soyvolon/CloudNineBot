using System.Threading.Tasks;

using CloudNine.Discord.Utilities;

using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;

namespace CloudNine.Discord.Commands.Birthday
{
    public class ForceChannelDescUpdateCmd : CommandModule
    {
        private readonly BirthdayManager _birthdays;

        public ForceChannelDescUpdateCmd(BirthdayManager birthdays)
        {
            _birthdays = birthdays;
        }

        [Command("forceupdate")]
        [RequireGuild]
        [Description("Forces the birthday channel description to update.")]
        [RequireUserPermissions(Permissions.ManageGuild)]
        public async Task ForceChannelDescUpdateAsync(CommandContext ctx)
        {
            if (await _birthdays.ForceChannelUpdate(ctx.Guild))
                await ctx.RespondAsync("Channel updated.");
            else await ctx.RespondAsync("Failed to update channel. Make sure a birthday channel is set.");
        }
    }
}