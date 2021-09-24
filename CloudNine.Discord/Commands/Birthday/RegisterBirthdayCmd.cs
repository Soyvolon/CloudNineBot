using System;
using System.Threading.Tasks;

using CloudNine.Discord.Utilities;

using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.SlashCommands;
using DSharpPlus.SlashCommands.Attributes;

namespace CloudNine.Discord.Commands.Birthday
{
    public partial class BirthdayCommands : SlashCommandBase
    {
        [SlashCommand("register", "Registers your birthday for the server you use this command on.")]
        [SlashRequireGuild]
        public async Task RegisterBirthdayAsync(InteractionContext ctx,
            [Option("Month", "What month is your birthday in?")]
            long month, 
            
            [Option("Day", "Day of the month")]
            long day)
        {
            await ctx.CreateResponseAsync(DSharpPlus.InteractionResponseType.DeferredChannelMessageWithSource);
            _birthdays.UpdateBirthday(ctx.Guild.Id, ctx.User.Id, new(DateTime.UtcNow.Year, (int)month, (int)day));
            await Respond($"Birthday on this server set to (Month/Day): {month}/{day}");
        }
    }
}
