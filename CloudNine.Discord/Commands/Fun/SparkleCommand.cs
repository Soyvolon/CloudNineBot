using System.Threading.Tasks;

using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;

namespace CloudNine.Discord.Commands.Fun
{
    public class SparkleCommand : SlashCommandBase
    {
        [SlashCommand("sparkle", "Makes your text sparkly")]
        public async Task SparkleCommandAsync(InteractionContext ctx,
            [Option("Text", "Text to sparkle")]
            string text)
        {
            await ctx.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource);

            if (text is null || text == "")
            {
                await RespondError("No text provided!");
                return;
            }

            string start = ":sparkles: ***";
            string end = "*** :sparkles:";

            var parts = text.ToCharArray();
            var body = string.Join(" ", parts);

            await Respond(start + body + end);
        }

        [SlashCommand("sparklecode", "Get the sparkle text formatting for your message!")]
        public async Task SparkleCodeCommandAsync(InteractionContext ctx,
            [Option("Test", "Text to sparkle")]
            string text)
        {
            await ctx.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource);

            if (text is null || text == "")
            {
                await RespondError("No text provided!");
                return;
            }

            string start = "```:sparkles: ***";
            string end = "*** :sparkles:```";

            var parts = text.ToCharArray();
            var body = string.Join(" ", parts);

            await Respond(start + body + end);
        }

        [SlashCommand("fuming", "Sparkle shortcut for the word fuming")]
        [RequireUserPermissions(Permissions.AccessChannels)]
        public async Task FumingCommandAsync(InteractionContext ctx)
            => await SparkleCommandAsync(ctx, "fuming");
    }
}
