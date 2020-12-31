using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using CloudNine.Core.Configuration;
using CloudNine.Core.Database;

using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;

using Microsoft.Extensions.DependencyInjection;

namespace CloudNine.Discord.Commands.Quotes.Favorites
{
    public class RemoveFavoriteQuoteCommand : CommandModule
    {
        private readonly IServiceProvider _services;

        public RemoveFavoriteQuoteCommand(IServiceProvider services)
        {
            this._services = services;
        }

        [Command("unfavoritequote")]
        [Description("Removes a quote from your favorite quotes for this server.")]
        [Aliases("unfavquote", "ufavq", "unfavouritequote")]
        public async Task RemoveFavoriteQuoteCommandAsync(CommandContext ctx,
            [Description("ID of the quote to remove from your favoites")]
            int quoteId)
        {
            var database = _services.GetRequiredService<CloudNineDatabaseModel>();

            var config = await database.FindAsync<DiscordGuildConfiguration>(ctx.Guild.Id);

            if (config is null || config.Quotes.IsEmpty)
            {
                await RespondError("No quotes are avalible to favorite on this server!");
                return;
            }

            if (config.Keys.Contains(quoteId))
            {
                bool removed;
                if (!config.FavoriteQuotes.ContainsKey(ctx.Member.Id))
                {
                    config.FavoriteQuotes[ctx.Member.Id] = new();
                    removed = false;
                }
                else
                {
                    removed = config.FavoriteQuotes[ctx.Member.Id].Remove(quoteId);
                }

                if (removed)
                {
                    database.Update(config);
                    await database.SaveChangesAsync();

                    await Respond($"Unfavorited quote {quoteId}.");
                }
                else
                {
                    await RespondError($"You don't have quote {quoteId} favorited.");
                }
            }
            else
            {
                await RespondError($"No quote by the id of {quoteId} has been found. Quotes that are deleted will automatically be removed" +
                    $" from your favorite quote list.");
            }
        }
    }
}
