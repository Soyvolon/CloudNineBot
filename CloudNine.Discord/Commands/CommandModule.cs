using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

using DSharpPlus.CommandsNext;
using DSharpPlus.Entities;

namespace CloudNine.Discord.Commands
{
    public class CommandModule : BaseCommandModule
    {
        public static readonly DiscordColor Color_Cloud = new DiscordColor(0x3498db);
        public static readonly DiscordColor Color_Warn = new DiscordColor(0xe07c10);
        public static readonly DiscordColor Color_Search = DiscordColor.Aquamarine;

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

        public async Task RespondWarn(string response)
        {
            await ctx.RespondAsync(embed: ModBase().WithDescription(response));
        }

        public async Task RespondError(string response)
        {
            await ctx.RespondAsync(embed: ErrorBase().WithDescription(response));
        }

        public static DiscordEmbedBuilder ErrorBase()
        {
            return new DiscordEmbedBuilder()
                .WithColor(DiscordColor.Red);
        }

        public static DiscordEmbedBuilder ModBase()
        {
            return new DiscordEmbedBuilder()
                .WithColor(Color_Warn);
        }

        public static DiscordEmbedBuilder InteractBase()
        {
            return new DiscordEmbedBuilder()
                .WithColor(Color_Cloud);
        }

        public async Task InteractTimeout(string message = "Interactivty Timed Out.")
        {
            var embed = ErrorBase()
                .WithDescription(message);

            await ctx.RespondAsync(embed: embed);
        }
    }
}
