using System;
using System.Threading.Tasks;

using CloudNine.Discord.Utilities;

using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;

namespace CloudNine.Discord.Commands.Birthday
{
    public class RegisterBirthdayCmd : CommandModule
    {
        private readonly BirthdayManager _birthdays;

        public RegisterBirthdayCmd(BirthdayManager birthdays)
        {
            _birthdays = birthdays;
        }

        [Command("register")]
        [RequireGuild]
        [Description("Registers your birthday for the server you use this command on.")]
        public async Task RegisterBirthdayAsync(CommandContext ctx,
            [Description("What month and day to set your brithday to.\nFormat: MM/DD or Month DD")]
        [RemainingText] DateTime date)
        {
            _birthdays.UpdateBirthday(ctx.Guild.Id, ctx.User.Id, date);
            await ctx.RespondAsync($"Birthday on this server set to (Month/Day): {date.Month}/{date.Day}");
        }
    }
}
