﻿using System;
using System.Threading.Tasks;

using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;

namespace BirthdayBotTesting.Commands
{
    public class RegisterBirthdayCmd : BaseCommandModule
    {

        [Command("register")]
        [Description("Registers your birthday for the server you use this command on.")]
        public async Task RegisterBirthdayAsync(CommandContext ctx,
            [Description("What month and day to set your brithday to.\nFormat: MM/DD or Month DD")]
        [RemainingText] DateTime date)
        {
            Program.Bot.Birthdays.UpdateBirthday(ctx.Guild.Id, ctx.User.Id, date);
            await ctx.RespondAsync($"Birthday on this server set to (Month/Day): {date.Month}/{date.Day}");
        }
    }
}
