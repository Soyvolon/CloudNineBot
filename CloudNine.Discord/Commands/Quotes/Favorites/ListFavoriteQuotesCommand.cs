using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using CloudNine.Core.Configuration;
using CloudNine.Core.Database;
using CloudNine.Core.Quotes;

using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity.Extensions;

using Microsoft.Extensions.DependencyInjection;

namespace CloudNine.Discord.Commands.Quotes.Favorites
{
    public class ListFavoriteQuotesCommand : CommandModule
    {
        private readonly IServiceProvider _services;

        public ListFavoriteQuotesCommand(IServiceProvider services)
        {
            this._services = services;
        }

        [Command("favoritequotes")]
        [Description("Lists the favorite quotes of a member.")]
        [Aliases("favoriteq", "qfavs", "favouritequotes")]
        public async Task ListFavoriteQuotesCommandAsync(CommandContext ctx,
            [Description("Member to list favorite quotes for. Defaults to yourself.")]
            DiscordMember? member = null)
        {
            DiscordMember mem;
            if (member is null) 
                mem = ctx.Member;
            else 
                mem = member;

            var database = _services.GetRequiredService<CloudNineDatabaseModel>();
            var cfg = await database.FindAsync<DiscordGuildConfiguration>(ctx.Guild.Id);

            if(cfg is null || cfg.FavoriteQuotes.IsEmpty)
            {
                await RespondError("No favorite quotes avalible to list on this server!");
                return;
            }

            if(cfg.FavoriteQuotes.TryGetValue(mem.Id, out var favorites))
            {
                List<Quote> quotes = new();

                foreach (var id in favorites)
                {
                    try
                    {
                        quotes.Add(cfg.Quotes[id]);
                    }
                    catch
                    {
                        cfg.FavoriteQuotes[mem.Id].Remove(id);
                    }
                }

                var interact = ctx.Client.GetInteractivity();
                var embedBase = new DiscordEmbedBuilder()
                .WithColor(Color_Cloud)
                .WithTitle($"Favorite Quotes for {mem.DisplayName}");

                var pages = ListQuotesCommand.GetQuotePages(quotes, interact, embedBase);

                _ = Task.Run(async () => await interact.SendPaginatedMessageAsync(ctx.Channel, ctx.Member, pages));
            }
            else
            {
                await RespondError($"No favorites for {mem.DisplayName} found.");
            }
        }
    }
}
