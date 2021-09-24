using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Threading.Tasks;

using CloudNine.Core.Configuration;
using CloudNine.Core.Database;
using CloudNine.Core.Quotes;
using CloudNine.Discord.Utilities;

using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity;
using DSharpPlus.Interactivity.Extensions;
using DSharpPlus.SlashCommands;
using DSharpPlus.SlashCommands.Attributes;

using Microsoft.Extensions.DependencyInjection;

namespace CloudNine.Discord.Commands.Quotes
{
    [SlashCommandGroup("list", "List related commands!")]
    public class ListQuotesCommand : SlashCommandBase
    {
        private readonly IServiceProvider _services;

        public ListQuotesCommand(IServiceProvider services)
        {
            this._services = services;
        }

        [SlashCommand("quotes", "Lists all quotes on this server.")]
        [SlashRequireGuild]
        public async Task ListQuotesCommandAsync(InteractionContext ctx)
        {
            var _database = _services.GetRequiredService<CloudNineDatabaseModel>();
            var cfg = await _database.FindAsync<DiscordGuildConfiguration>(ctx.Guild.Id);
            if (cfg is null)
            {
                cfg = new DiscordGuildConfiguration()
                {
                    Id = ctx.Guild.Id
                };

                await _database.AddAsync(cfg);
                await _database.SaveChangesAsync();
            }

            var embedBase = new DiscordEmbedBuilder()
                .WithColor(Color_Cloud)
                .WithTitle($"Quotes saved on {ctx.Guild.Name}");

            var quoteList = cfg.Quotes.Values.ToImmutableSortedSet(new QuoteComparer());

            await ctx.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource);

            var interact = ctx.Client.GetInteractivity();

            var pages = GetQuotePages(quoteList, interact, embedBase);

            _ = Task.Run(async () => await interact.SendPaignatedMessageWithButtonsAsync(ctx, ctx.User, pages));
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

        [SlashCommand("favorite", "Lists the favorite quotes of a member.")]
        [SlashRequireGuild]
        public async Task ListFavoriteQuotesCommandAsync(InteractionContext ctx,
            [Option("User", "Member to list favorite quotes for. Defaults to yourself.")]
            DiscordUser? member = null)
        {
            await ctx.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource);

            DiscordUser mem;
            if (member is null)
                mem = ctx.User;
            else
                mem = member;

            var database = _services.GetRequiredService<CloudNineDatabaseModel>();
            var cfg = await database.FindAsync<DiscordGuildConfiguration>(ctx.Guild.Id);

            if (cfg is null || cfg.FavoriteQuotes.IsEmpty)
            {
                await RespondError("No favorite quotes avalible to list on this server!");
                return;
            }

            if (mem.Id == ctx.Client.CurrentApplication.Id)
            {
                var list = new ListQuotesCommand(_services);
                await list.BeforeSlashExecutionAsync(ctx);
                await list.ListQuotesCommandAsync(ctx);
                return;
            }

            if (cfg.FavoriteQuotes.TryGetValue(mem.Id, out var favorites))
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
                .WithTitle($"Favorite Quotes for {mem.Username}");

                var pages = ListQuotesCommand.GetQuotePages(quotes, interact, embedBase);

                _ = Task.Run(async () => await interact.SendPaignatedMessageWithButtonsAsync(ctx, ctx.Member, pages));
            }
            else
            {
                await RespondError($"No favorites for {mem.Username} found.");
            }
        }

        [SlashCommand("hidden", "Lists the hidden quotes.")]
        [SlashRequireGuild]
        [SlashRequireUserPermissions(Permissions.ManageMessages)]
        public async Task ListHiddenQuotesCommandAsync(InteractionContext ctx)
        {
            var _database = _services.GetRequiredService<CloudNineDatabaseModel>();
            var cfg = await _database.FindAsync<DiscordGuildConfiguration>(ctx.Guild.Id);
            if (cfg is null)
            {
                cfg = new DiscordGuildConfiguration()
                {
                    Id = ctx.Guild.Id
                };

                await _database.AddAsync(cfg);
                await _database.SaveChangesAsync();
            }

            string data = "";
            var embedBase = new DiscordEmbedBuilder()
                .WithColor(Color_Cloud)
                .WithTitle($"Quotes saved on {ctx.Guild.Name}");

            var quoteList = cfg.HiddenQuotes.Values.ToImmutableSortedSet(new HiddenQuoteComparer());

            int c = 1;
            foreach (var quote in quoteList)
            {
                var content = quote.Content.Length > 23 ? quote.Content[0..20] + "..." : quote.Content;
                data += $"{c++}-ID: `{quote.CustomId}`: {content} by `{quote.Author}`\n";
            }

            await ctx.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource);

            var interact = ctx.Client.GetInteractivity();

            var pages = interact.GeneratePagesInEmbed(data, DSharpPlus.Interactivity.Enums.SplitType.Line, embedBase);

            _ = interact.SendPaignatedMessageWithButtonsAsync(ctx, ctx.Member, pages);
        }
    }
}
