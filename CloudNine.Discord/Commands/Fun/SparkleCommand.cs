using System.Threading.Tasks;

using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;

namespace CloudNine.Discord.Commands.Fun
{
    public class SparkleCommand : BaseCommandModule
    {
        [Command("sparkle")]
        [Description("Makes your text sparkly")]
        public async Task SparkleCommandAsync(CommandContext ctx,
            [Description("Text to sparkle")]
            [RemainingText]
            string text)
        {
            string start = ":sparkles: ***";
            string end = "*** :sparkles:";

            var parts = text.ToCharArray();
            var body = string.Join(" ", parts);

            await ctx.RespondAsync(start + body + end);
        }

        [Command("sparklecode")]
        [Description("Get the sparkle text formatting for your message!")]
        public async Task SparkleCodeCommandAsync(CommandContext ctx,
            [Description("Text to sparkle")]
            [RemainingText]
            string text)
        {
            string start = "```:sparkles: ***";
            string end = "*** :sparkles:```";

            var parts = text.ToCharArray();
            var body = string.Join(" ", parts);

            await ctx.RespondAsync(start + body + end);
        }
    }
}
