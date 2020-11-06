using System.Threading.Tasks;

using DSharpPlus.CommandsNext;
using DSharpPlus.Entities;

namespace CloudNine.Discord.Commands
{
    public class CommandModule : BaseCommandModule
    {
        public static readonly DiscordColor Color_Cloud = new DiscordColor(0x3498db);

        private CommandContext ctx;

        public override Task BeforeExecutionAsync(CommandContext ctx)
        {
            this.ctx = ctx;
            return base.BeforeExecutionAsync(ctx);
        }

        public async Task Respond(string response)
        {
            await ctx.RespondAsync(response);
        }
    }
}
