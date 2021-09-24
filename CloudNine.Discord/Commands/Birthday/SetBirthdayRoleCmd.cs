using System.Threading.Tasks;

using CloudNine.Discord.Utilities;

using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using DSharpPlus.SlashCommands.Attributes;

namespace CloudNine.Discord.Commands.Birthday
{
    public partial class BirthdayCommands : SlashCommandBase
    {
        [SlashCommand("role", "Sets the role to be given to a user whos birthday is today.")]
        [SlashRequireGuild]
        [SlashRequireUserPermissions(Permissions.ManageRoles)]
        public async Task SetBirthdayRoleAsync(InteractionContext ctx,
            [Option("Role", "Role to give the birthday person.")]
            DiscordRole role)
        {
            await ctx.CreateResponseAsync(DSharpPlus.InteractionResponseType.DeferredChannelMessageWithSource);
            _birthdays.UpdateBirthdayRole(ctx.Guild.Id, role);
            await Respond($"Set birthday role to {role.Mention}");
        }
    }
}
