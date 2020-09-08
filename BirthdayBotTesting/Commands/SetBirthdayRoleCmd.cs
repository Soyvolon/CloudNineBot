using System.Threading.Tasks;

using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;

namespace BirthdayBotTesting.Commands
{
    public class SetBirthdayRoleCmd : BaseCommandModule
    {
        [Command("role")]
        [Description("Sets the role to be given to a user whos birthday is today.")]
        [RequireUserPermissions(DSharpPlus.Permissions.Administrator)]
        public async Task SetBirthdayRoleAsync(CommandContext ctx, DiscordRole role)
        {
            Program.Bot.Birthdays.UpdateBirthdayRole(ctx.Guild.Id, role);
            await ctx.RespondAsync($"Set birthday role to {role.Mention}").ConfigureAwait(false);
        }
    }
}
