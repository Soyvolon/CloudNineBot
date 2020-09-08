using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;

namespace BirthdayBotTesting.Commands
{
    public class AdminOverrides : BaseCommandModule
    {
        [Command("aregister")]
        [RequireUserPermissions(DSharpPlus.Permissions.Administrator)]
        public async Task AdminRegisterUser(CommandContext ctx, DiscordUser m, [RemainingText] DateTime bday)
        {
            Program.Bot.Birthdays.UpdateBirthday(ctx.Guild.Id, m.Id, bday);
            await ctx.RespondAsync($"Updated {m.Username}'s bday to {bday:dd MMMM}");
        }

        [Command("aremove")]
        [RequireUserPermissions(DSharpPlus.Permissions.Administrator)]
        public async Task AdminRemoveUser(CommandContext ctx, DiscordUser user)
        {
            Program.Bot.Birthdays.RemoveBirthday(ctx.Guild.Id, user.Id);
            await ctx.RespondAsync($"Removed {user.Username}'s bday.");
        }
    }
}
