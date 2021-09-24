using System;
using System.Threading.Tasks;

using CloudNine.Core.Configuration;
using CloudNine.Core.Database;

using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using DSharpPlus.SlashCommands.Attributes;

using Microsoft.Extensions.DependencyInjection;

namespace CloudNine.Discord.Commands.Quotes.Favorites
{
    [SlashCommandGroup("favorite", "Commands for favoriting quotes!")]
    public class AddFavoriteQuoteCommand : SlashCommandBase
    {
        private readonly IServiceProvider _services;

        public AddFavoriteQuoteCommand(IServiceProvider services)
        {
            this._services = services;
        }

        [SlashCommand("quote", "Add a quote to your favorite quotes for this server!")]
        [SlashRequireGuild]
        public async Task AddFavoriteQuoteCommandAsync(InteractionContext ctx, 
            [Option("ID", "ID of the quote to favorite")]
            long quoteLong)
        {
            int quoteId = (int)quoteLong;

            var database = _services.GetRequiredService<CloudNineDatabaseModel>();

            var config = await database.FindAsync<DiscordGuildConfiguration>(ctx.Guild.Id);

            await ctx.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource);

            if(config is null || config.Quotes.IsEmpty)
            {
                await RespondError("No quotes are avalible to favorite on this server!");
                return;
            }

            if(config.Keys.Contains(quoteId))
            {
                bool added;
                if (!config.FavoriteQuotes.ContainsKey(ctx.Member.Id))
                {
                    config.FavoriteQuotes[ctx.Member.Id] = new() { quoteId };
                    added = true;
                }
                else
                {
                    added = config.FavoriteQuotes[ctx.Member.Id].Add(quoteId);
                }

                if(added)
                {
                    database.Update(config);
                    await database.SaveChangesAsync();

                    var embed = new DiscordEmbedBuilder();
                    embed.WithColor(Color_Cloud)
                        .WithDescription($"Favorited quote {quoteId}!");

                    await RespondAsync(embed);
                }
                else
                {
                    await RespondError($"You already have quote {quoteId} favorited!");
                }
            }
            else
            {
                await RespondError($"No quote by the id of {quoteId} has been found.");
            }
        }
    }
}
