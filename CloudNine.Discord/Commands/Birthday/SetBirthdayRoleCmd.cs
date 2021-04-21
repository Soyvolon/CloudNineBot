using System.Threading.Tasks;

using CloudNine.Discord.Utilities;

using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;

namespace CloudNine.Discord.Commands.Birthday
{
    public class SetBirthdayRoleCmd : CommandModule
    {
        private readonly BirthdayManager _birthdays;

        public SetBirthdayRoleCmd(BirthdayManager birthdays)
        {
            _birthdays = birthdays;
        }

        [Command("role")]
        [RequireGuild]
        [Hidden]
        [Description("Sets the role to be given to a user whos birthday is today.")]
        [RequireUserPermissions(Permissions.ManageGuild)]
        public async Task SetBirthdayRoleAsync(CommandContext ctx, DiscordRole role)
        {
            _birthdays.UpdateBirthdayRole(ctx.Guild.Id, role);
            await ctx.RespondAsync($"Set birthday role to {role.Mention}").ConfigureAwait(false);
        }
    }
}
