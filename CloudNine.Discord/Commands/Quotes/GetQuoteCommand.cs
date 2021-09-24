using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

using CloudNine.Core.Configuration;
using CloudNine.Core.Database;
using CloudNine.Core.Extensions;

using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.Exceptions;
using DSharpPlus.SlashCommands;
using DSharpPlus.SlashCommands.Attributes;

using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;

namespace CloudNine.Discord.Commands.Quotes
{
    public class GetQuoteCommand : SlashCommandBase
    {
        private readonly IServiceProvider _services;

        public GetQuoteCommand(IServiceProvider services)
        {
            this._services = services;
        }

        [SlashCommand("random", "Gets a random quote!")]
        [SlashRequireGuild]
        public async Task GetQuoteCommandAsync(InteractionContext ctx)
            => await GetQuoteCommandAsync(ctx, "");

        [SlashCommand("quote", "Gets a saved quote! Use /random to get a random quote!")]
        [SlashRequireGuild]
        public async Task GetQuoteCommandAsync(InteractionContext ctx,
            [Option("ID", "ID of the quote to get!")]
            string rawArgs)
        {
            await ctx.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource);

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

            var args = rawArgs.Split(' ', StringSplitOptions.RemoveEmptyEntries);

            if (args is null || args.Length <= 0)
            {
                if(cfg.Keys.Count == 0)
                {
                    await RespondError("No quotes found!");
                    return;
                }

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
                        $"Using `/quote` without anything else will get you a random quote from this server.")
                    .AddField("help", "```http\n" +
                        $"Usage     :: /quote help\n" +
                        $"Returns   :: This embed." +
                        $"\n```")
                    .AddField("id", "```http\n" +
                        $"Usage     :: /quote id <quote id>\n" +
                        $"Quote Id  :: The ID of the quote you want to get.\n" +
                        $"Returns   :: A specific quote." +
                        $"\n```");
                /*.AddField("global", "```http\n" +
                    $"Usage     :: /quote global\n" +
                    $"Returns   :: A random globaly shared quote." +
                    $"\n```");*/

                await RespondAsync(embed: embed);
            }
            else if (args[0] == "id")
            {
                if (args.Length < 2)
                {
                    await Respond("No ID provided. Make sure to include an ID for the quote you want to get.");
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
                    await Respond("Could not parse the provided ID. Please input a number value. `Ex: 5`");
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
                await RespondError("No quote found!");
            }

            await _database.SaveChangesAsync();
        }

        private async Task SendHiddenQuoteById(InteractionContext ctx, DiscordGuildConfiguration cfg, string quoteId)
        {
            if (cfg.HiddenQuotes.TryGetValue(quoteId, out var quote))
            {
                var embed = quote.UseQuote();

                await RespondAsync(embed: embed);
            }
            else
            {
                await Respond("No quote by that ID found.");
            }
        }

        private async Task SendQuoteByIdAsync(InteractionContext ctx, DiscordGuildConfiguration cfg, int quoteId)
        {
            if (cfg.Quotes.TryGetValue(quoteId, out var quote))
            {
                var embed = quote.UseQuote();

                await RespondAsync(embed: embed);
            }
            else
            {
                await RespondError("No quote by that ID found.");
            }
        }
    }
}
