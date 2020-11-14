using System;
using System.Threading.Tasks;

using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;

namespace CloudNine.Discord.Commands.Birthday
{
    public class AdminOverrides : CommandModule
    {
        [Command("aregister")]
        [RequireGuild]
        [RequireUserPermissions(Permissions.ManageGuild)]
        public async Task AdminRegisterUser(CommandContext ctx, DiscordUser m, [RemainingText] DateTime bday)
        {
            DiscordBot.Bot.Birthdays.UpdateBirthday(ctx.Guild.Id, m.Id, bday);
            await ctx.RespondAsync($"Updated {m.Username}'s bday to {bday:dd MMMM}");
        }

        [Command("aremove")]
        [RequireUserPermissions(Permissions.ManageGuild)]
        public async Task AdminRemoveUser(CommandContext ctx, DiscordUser user)
        {
            DiscordBot.Bot.Birthdays.RemoveBirthday(ctx.Guild.Id, user.Id);
            await ctx.RespondAsync($"Removed {user.Username}'s bday.");
        }

        [Command("testperms")]
        [Hidden]
        [RequireOwner]
        public async Task TestPermsAsync(CommandContext ctx)
        {
            await ctx.Channel.AddOverwriteAsync(ctx.Member, DSharpPlus.Permissions.AccessChannels, DSharpPlus.Permissions.AddReactions, "testing");
            await DiscordBot.Bot.Rest.EditChannelPermissionsAsync(ctx.Channel.Id, ctx.Member.Id, DSharpPlus.Permissions.AddReactions, DSharpPlus.Permissions.AccessChannels, "member", "Testing");
        }
    }
}
