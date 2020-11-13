using System.Threading.Tasks;

using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;

namespace CloudNine.Discord.Commands.Devine
{
    public class DevineCommandLine : CommandModule
    {
        [Command("devine")]
        [Description("Devine centric command line.")]
        [Hidden]
        public async Task DevineCommandLineAsync(CommandContext ctx,
            [Description("Command Line Arguments")]
            params string[] args)
        {

        }

        private async Task DevineCommandLineHelpAsync(CommandContext ctx)
        {

        }
    }
}
