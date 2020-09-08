using System.Threading.Tasks;

using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;

namespace BirthdayBotTesting.Commands
{
    public class TriggerNextTimerEventCmd : BaseCommandModule
    {
        [Command("trigger")]
        [RequireOwner]
        [Hidden]
        public async Task DebugTriggerAsync(CommandContext ctx)
        {
            if (Program.IsDebug)
            {
                Program.Bot.Birthdays.DebugTrigger = true;
                await ctx.RespondAsync("Triggered").ConfigureAwait(false);
            }
        }
    }
}
