using System.Threading.Tasks;

using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;

namespace CloudNine.Discord.Commands.Birthday
{
    public class TriggerNextTimerEventCmd : CommandModule
    {
        [Command("trigger")]
        [RequireOwner]
        [Hidden]
        public async Task DebugTriggerAsync(CommandContext ctx)
        {
            if (DiscordBot.IsDebug)
            {
                DiscordBot.Bot.Birthdays.DebugTrigger = true;
                await ctx.RespondAsync("Triggered").ConfigureAwait(false);
            }
        }
    }
}
