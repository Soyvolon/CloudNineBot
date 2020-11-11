using System.Threading.Tasks;

using CloudNine.Core.Database;

using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;

namespace CloudNine.Discord.Commands.Quotes
{
    public class SearchQuotesCommand : BaseCommandModule
    {
        private readonly CloudNineDatabaseModel _database;

        public SearchQuotesCommand(CloudNineDatabaseModel database)
        {
            this._database = database;
        }

        [Command("searchquotes")]
        [Description("Searches quotes by a specific serach")]
        [Aliases("searchquote", "quoteserach")]
        public async Task SearchQuotesCommandAsync(CommandContext ctx,
            [Description("Quote serach arguments.")]
            params string[] args)
        {

        }
    }
}
