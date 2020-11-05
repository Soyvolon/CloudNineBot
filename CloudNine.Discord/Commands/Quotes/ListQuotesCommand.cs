using System.Threading.Tasks;

using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;

namespace CloudNine.Discord.Commands.Quotes
{
    public class ListQuotesCommand : CommandModule
    {
        [Command("listquotes")]
        [Description("Lists all quotes on this server.")]
        [Aliases("lquotes")]
        public async Task ExampleCommandAsync(CommandContext ctx)
        {

        }
    }
}
