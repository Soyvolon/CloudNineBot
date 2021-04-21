using System.Threading.Tasks;

using CloudNine.Discord.Utilities;

using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;

namespace CloudNine.Discord.Commands.Birthday
{
    public class TriggerNextTimerEventCmd : CommandModule
    {
        private readonly BirthdayManager _birthdays;

        public TriggerNextTimerEventCmd(BirthdayManager birthdays)
        {
            _birthdays = birthdays;
        }

        [Command("bdaytrigger")]
        [RequireGuild]
        [RequireOwner]
        [Hidden]
        public async Task DebugTriggerAsync(CommandContext ctx)
        {
            if (DiscordBot.IsDebug)
            {
                _birthdays.DebugTrigger = true;
                await ctx.RespondAsync("Triggered").ConfigureAwait(false);
            }
        }
    }
}
