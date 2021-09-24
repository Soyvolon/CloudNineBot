using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using CloudNine.Core.Configuration;
using CloudNine.Core.Database;

using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.SlashCommands;
using DSharpPlus.SlashCommands.Attributes;

using Microsoft.Extensions.DependencyInjection;

namespace CloudNine.Discord.Commands.Quotes.Favorites
{
    [SlashCommandGroup("unfavorite", "Unfavorite commands.")]
    public class RemoveFavoriteQuoteCommand : SlashCommandBase
    {
        private readonly IServiceProvider _services;

        public RemoveFavoriteQuoteCommand(IServiceProvider services)
        {
            this._services = services;
        }

        [SlashCommand("quote", "Removes a quote from your favorite quotes for this server.")]
        [SlashRequireGuild]
        public async Task RemoveFavoriteQuoteCommandAsync(InteractionContext ctx,
            [Option("ID", "ID of the quote to remove from your favoites")]
            long quoteLong)
        {
            var quoteId = (int)quoteLong;

            var database = _services.GetRequiredService<CloudNineDatabaseModel>();

            var config = await database.FindAsync<DiscordGuildConfiguration>(ctx.Guild.Id);

            await ctx.CreateResponseAsync(DSharpPlus.InteractionResponseType.DeferredChannelMessageWithSource);

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
