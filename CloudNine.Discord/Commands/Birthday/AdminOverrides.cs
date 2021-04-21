using System;
using System.Threading.Tasks;

using CloudNine.Discord.Utilities;

using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;

namespace CloudNine.Discord.Commands.Birthday
{
    public class AdminOverrides : CommandModule
    {
        private readonly BirthdayManager _birthdays;
        private readonly DiscordRestClient _rest;

        public AdminOverrides(BirthdayManager birthdays, DiscordRestClient rest)
        {
            _birthdays = birthdays;
            _rest = rest;
        }

        [Command("aregister")]
        [RequireGuild]
        [RequireUserPermissions(Permissions.ManageGuild)]
        public async Task AdminRegisterUser(CommandContext ctx, DiscordUser m, [RemainingText] DateTime bday)
        {
            _birthdays.UpdateBirthday(ctx.Guild.Id, m.Id, bday);
            await ctx.RespondAsync($"Updated {m.Username}'s bday to {bday:dd MMMM}");
        }

        [Command("aremove")]
        [RequireUserPermissions(Permissions.ManageGuild)]
        public async Task AdminRemoveUser(CommandContext ctx, DiscordUser user)
        {
            _birthdays.RemoveBirthday(ctx.Guild.Id, user.Id);
            await ctx.RespondAsync($"Removed {user.Username}'s bday.");
        }

        [Command("testperms")]
        [Hidden]
        [RequireOwner]
        public async Task TestPermsAsync(CommandContext ctx)
        {
            await ctx.Channel.AddOverwriteAsync(ctx.Member, DSharpPlus.Permissions.AccessChannels, DSharpPlus.Permissions.AddReactions, "testing");
            await _rest.EditChannelPermissionsAsync(ctx.Channel.Id, ctx.Member.Id, DSharpPlus.Permissions.AddReactions, DSharpPlus.Permissions.AccessChannels, "member", "Testing");
        }
    }
}
