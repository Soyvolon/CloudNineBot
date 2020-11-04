using System.Threading.Tasks;

using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;

namespace CloudNine.Discord.Commands.Birthday
{
    public class UnRegisterBirthdayCmd : BaseCommandModule
    {
        [Command("remove")]
        [Aliases("unregister")]
        [Description("Removes you from the birthday list on this server.")]
        public async Task UnregisterBirthdayAsync(CommandContext ctx)
        {
            if (DiscordBot.Bot.Birthdays.RemoveBirthday(ctx.Guild.Id, ctx.Member.Id))
                await ctx.RespondAsync("Birthday removed sucsessfuly!").ConfigureAwait(false);
            else
                await ctx.RespondAsync("You don't have a birthday registered on this server.").ConfigureAwait(false);
        }
    }
}