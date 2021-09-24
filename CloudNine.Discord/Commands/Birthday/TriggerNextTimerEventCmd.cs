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
        [SlashCommand("trigger", "Tests bday execution")]
        [SlashRequireGuild]
        [SlashRequireOwner]
        public async Task DebugTriggerAsync(InteractionContext ctx)
        {
            if (DiscordBot.IsDebug)
            {
                await ctx.CreateResponseAsync(DSharpPlus.InteractionResponseType.DeferredChannelMessageWithSource);
                _birthdays.DebugTrigger = true;
                await Respond("Triggered");
            }
        }
    }
}
