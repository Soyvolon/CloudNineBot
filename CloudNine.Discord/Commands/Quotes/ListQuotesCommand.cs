using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Threading.Tasks;

using CloudNine.Core.Configuration;
using CloudNine.Core.Database;
using CloudNine.Core.Quotes;
using CloudNine.Discord.Utilities;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity;
using DSharpPlus.Interactivity.Extensions;

using Microsoft.Extensions.DependencyInjection;

namespace CloudNine.Discord.Commands.Quotes
{
    public class ListQuotesCommand : CommandModule
    {
        private readonly IServiceProvider _services;

        public ListQuotesCommand(IServiceProvider services)
        {
            this._services = services;
        }

        [Command("listquotes")]
        [RequireGuild]
        [Description("Lists all quotes on this server.")]
        [Aliases("lquotes")]
        public async Task ListQuotesCommandAsync(CommandContext ctx)
        {
            var _database = _services.GetRequiredService<CloudNineDatabaseModel>();
            var cfg = await _database.FindAsync<DiscordGuildConfiguration>(ctx.Guild.Id);
            if (cfg is null)
            {
                cfg = new DiscordGuildConfiguration()
                {
                    Id = ctx.Guild.Id,
                    Prefix = ctx.Prefix
                };

                await _database.AddAsync(cfg);
                await _database.SaveChangesAsync();
            }

            var embedBase = new DiscordEmbedBuilder()
                .WithColor(Color_Cloud)
                .WithTitle($"Quotes saved on {ctx.Guild.Name}");

            var quoteList = cfg.Quotes.Values.ToImmutableSortedSet(new QuoteComparer());

            var interact = ctx.Client.GetInteractivity();

            var pages = GetQuotePages(quoteList, interact, embedBase);

            _ = Task.Run(async () => await interact.SendPaignatedMessageWithButtonsAsync(ctx.Channel, ctx.User, pages));
        }

        public static IEnumerable<Page>? GetQuotePages(IEnumerable<Quote> quoteList, InteractivityExtension interact, DiscordEmbedBuilder embedBase)
        {
            string data = "";

            foreach (var quote in quoteList)
            {
                var content = quote.Content.Length > 53 ? quote.Content[0..50] + "..." : quote.Content;
                data += $"`{quote.Id}`. {content} by `{quote.Author}`\n";
            }

            var pages = interact.GeneratePagesInEmbed(data, DSharpPlus.Interactivity.Enums.SplitType.Line, embedBase);

            return pages;
        }
    }
}
