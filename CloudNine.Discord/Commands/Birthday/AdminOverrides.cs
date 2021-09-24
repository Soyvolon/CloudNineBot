using System;
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
    [SlashCommandGroup("bday", "Birthday Commands!")]
    public partial class BirthdayCommands : SlashCommandBase
    {
        private readonly BirthdayManager _birthdays;
        private readonly DiscordRestClient _rest;

        public BirthdayCommands(BirthdayManager birthdays, DiscordRestClient rest)
        {
            _birthdays = birthdays;
            _rest = rest;
        }

        [SlashCommand("adminregister", "Admin register command.")]
        [SlashRequireGuild]
        [SlashRequireUserPermissions(Permissions.ManageGuild)]
        public async Task AdminRegisterUser(InteractionContext ctx,
            [Option("User", "User to add a birthday for.")]
            DiscordUser m,
            [Option("Month", "What month is your birthday in?")]
            long month,

            [Option("Day", "Day of the month")]
            long day)
        {
            await ctx.CreateResponseAsync(DSharpPlus.InteractionResponseType.DeferredChannelMessageWithSource);
            DateTime bday = new(DateTime.UtcNow.Year, (int)month, (int)day);
            _birthdays.UpdateBirthday(ctx.Guild.Id, m.Id, bday);
            await Respond($"Updated {m.Username}'s bday to {bday:dd MMMM}");
        }

        [SlashCommand("adminremove", "Admin remove command.")]
        [SlashRequireUserPermissions(Permissions.ManageGuild)]
        public async Task AdminRemoveUser(InteractionContext ctx, 
            [Option("User", "User to remove a birthday for.")]
            DiscordUser user)
        {
            await ctx.CreateResponseAsync(DSharpPlus.InteractionResponseType.DeferredChannelMessageWithSource);
            _birthdays.RemoveBirthday(ctx.Guild.Id, user.Id);
            await Respond($"Removed {user.Username}'s bday.");
        }

        [SlashCommand("testperms", "Tests bot permissions.")]
        [SlashRequireOwner]
        public async Task TestPermsAsync(InteractionContext ctx)
        {
            await ctx.CreateResponseAsync(DSharpPlus.InteractionResponseType.DeferredChannelMessageWithSource);
            await ctx.Channel.AddOverwriteAsync(ctx.Member, DSharpPlus.Permissions.AccessChannels, DSharpPlus.Permissions.AddReactions, "testing");
            await _rest.EditChannelPermissionsAsync(ctx.Channel.Id, ctx.Member.Id, DSharpPlus.Permissions.AddReactions, DSharpPlus.Permissions.AccessChannels, "member", "Testing");
        }
    }
}
