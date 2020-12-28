using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

using CloudNine.Core.Configuration;
using CloudNine.Core.Database;
using CloudNine.Core.Extensions;

using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.Exceptions;

using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;

namespace CloudNine.Discord.Commands.Quotes
{
    public class GetQuoteCommand : CommandModule
    {
        private readonly IServiceProvider _services;

        public GetQuoteCommand(IServiceProvider services)
        {
            this._services = services;
        }

        [Command("quote")]
        [RequireGuild]
        [Description("Gets a saved quote!")]
        [Aliases("getquote")]
        public async Task GetQuoteCommandAsync(CommandContext ctx,
            [Description("Arguments for the quote command. Use `help` for more information")]
            params string[] args)
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

            if (args is null || args.Length <= 0)
            {
                var quoteId = cfg.Keys.Random();
                await SendQuoteByIdAsync(ctx, cfg, quoteId);
                _database.Update(cfg);
            }
            else if (args[0].ToLower() == "help")
            { // Display command help information.
                var embed = new DiscordEmbedBuilder()
                    .WithColor(Color_Cloud)
                    .WithTitle("Quote Help")
                    .WithDescription("Detailed help for the quote command.\n" +
                        $"Using `{ctx.Prefix}quote` without anything else will get you a random quote from this server.")
                    .AddField("help", "```http\n" +
                        $"Usage     :: {ctx.Prefix}quote help\n" +
                        $"Returns   :: This embed." +
                        $"\n```")
                    .AddField("id", "```http\n" +
                        $"Usage     :: {ctx.Prefix}quote id <quote id>\n" +
                        $"Quote Id  :: The ID of the quote you want to get.\n" +
                        $"Returns   :: A specific quote." +
                        $"\n```");
                /*.AddField("global", "```http\n" +
                    $"Usage     :: {ctx.Prefix}quote global\n" +
                    $"Returns   :: A random globaly shared quote." +
                    $"\n```");*/

                await ctx.RespondAsync(embed: embed);
            }
            else if (args[0] == "id")
            {
                if (args.Length < 2)
                {
                    await ctx.RespondAsync("No ID provided. Make sure to include an ID for the quote you want to get.");
                }
                else if (int.TryParse(args[1], out int id))
                {
                    await SendQuoteByIdAsync(ctx, cfg, id);
                    _database.Update(cfg);
                }
                else if (cfg.HiddenQuotes.ContainsKey(args[1]))
                {
                    await SendHiddenQuoteById(ctx, cfg, args[1]);
                    _database.Update(cfg);
                }
                else
                {
                    await ctx.RespondAsync("Could not parse the provided ID. Please input a number value. `Ex: 5`");
                }
            }
            else if (int.TryParse(args[0], out int id))
            {
                await SendQuoteByIdAsync(ctx, cfg, id);
                _database.Update(cfg);
            }
            else if (cfg.HiddenQuotes.ContainsKey(args[0]))
            {
                await SendHiddenQuoteById(ctx, cfg, args[0]);
                _database.Update(cfg);
            }
            else
            {
                await ctx.RespondAsync("No quote found!");
            }

            await _database.SaveChangesAsync();
        }

        private async Task SendHiddenQuoteById(CommandContext ctx, DiscordGuildConfiguration cfg, string quoteId)
        {
            if (cfg.HiddenQuotes.TryGetValue(quoteId, out var quote))
            {
                try
                {
                    await ctx.Message.DeleteAsync();
                }
                catch (NotFoundException) { }
                var embed = quote.UseQuote();

                await ctx.RespondAsync(embed: embed);
            }
            else
            {
                await ctx.RespondAsync("No quote by that ID found.");
            }
        }

        private async Task SendQuoteByIdAsync(CommandContext ctx, DiscordGuildConfiguration cfg, int quoteId)
        {
            if (cfg.Quotes.TryGetValue(quoteId, out var quote))
            {
                var embed = quote.UseQuote();

                await ctx.RespondAsync(embed: embed);
            }
            else
            {
                await ctx.RespondAsync("No quote by that ID found.");
            }
        }
    }
}
