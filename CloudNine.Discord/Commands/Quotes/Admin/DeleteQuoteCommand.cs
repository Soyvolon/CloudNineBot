using System.Threading.Tasks;

using CloudNine.Core.Configuration;
using CloudNine.Core.Database;

using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;

namespace CloudNine.Discord.Commands.Quotes.Admin
{
    public class DeleteQuoteCommand : CommandModule
    {
        private readonly CloudNineDatabaseModel _database;

        public DeleteQuoteCommand(CloudNineDatabaseModel database)
        {
            this._database = database;
        }

        [Command("deletequote")]
        [Description("Deltes a quote by its ID.")]
        [Aliases("delquote", "dquote")]
        [RequireUserPermissions(Permissions.ManageMessages)]
        public async Task DeleteQuoteCommandAsync(CommandContext ctx, 
            [Description("The ID of the quote to remove.")]
            int quoteId)
        {
            var cfg = await _database.FindAsync<DiscordGuildConfiguration>(ctx.Guild.Id);

            if(cfg.TryRemoveQuote(quoteId, out var quote))
            {
                var embed = new DiscordEmbedBuilder()
                    .WithTitle($"Remved Quote `{quoteId}` by {quote.Author}")
                    .WithDescription(quote.Content);

                await ctx.RespondAsync(embed: embed);
            }
            else
            {
                await Respond($"Failed to find a quote with the ID: `{quoteId}`");
            }
        }
    }
}
