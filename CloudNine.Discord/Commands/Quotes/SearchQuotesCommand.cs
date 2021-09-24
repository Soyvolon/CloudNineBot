using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using CloudNine.Core.Configuration;
using CloudNine.Core.Database;
using CloudNine.Discord.Services;

using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity.Enums;
using DSharpPlus.Interactivity.EventHandling;
using DSharpPlus.Interactivity.Extensions;
using DSharpPlus.SlashCommands;
using DSharpPlus.SlashCommands.Attributes;

using Microsoft.EntityFrameworkCore.Query.SqlExpressions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualBasic;
using Microsoft.VisualBasic.CompilerServices;

namespace CloudNine.Discord.Commands.Quotes
{
    [SlashCommandGroup("search", "Searching group!")]
    public class SearchQuoteGroup : SlashCommandBase
    {
        protected readonly IServiceProvider _services;
        protected readonly QuoteService _quotes;

        public SearchQuoteGroup(IServiceProvider services, QuoteService quotes)
        {
            this._services = services;
            this._quotes = quotes;
        }

        [SlashCommand("quote", "Searches quotes by a specific serach")]
        [SlashRequireGuild]
        public async Task SearchQuotesCommandAsync(InteractionContext ctx,
            [Option("Arguments", "Quote serach arguments. Use --help for help.")]
            string rawArgs)
        {
            await ctx.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource);
            var _database = _services.GetRequiredService<CloudNineDatabaseModel>();
            var cfg = await _database.FindAsync<DiscordGuildConfiguration>(ctx.Guild.Id);
            if (cfg is null)
            {
                await RespondError("No quotes found.");
                return;
            }

            var args = rawArgs.Split(' ', StringSplitOptions.RemoveEmptyEntries);

            if (args.Length <= 0 || args[0] == "-h" || args[0] == "--help")
            {
                await SearchQuotesHelpAsync(ctx);
                return;
            }

            List<HashSet<int>> matches = new List<HashSet<int>>();

            for (int i = 0; i < args.Length; i++)
            {
                var set = new HashSet<int>();

                switch (args[i])
                {
                    case "-a":
                    case "--author":
                        if (args.Length <= i + 1)
                        {
                            await RespondError("Failed to parse `--author`, not enough paramaters.");
                            return;
                        }

                        i++; // incrment the value so it wont be continuously intcrmented in the search.
                        var res = cfg.Quotes.Where(x =>
                        {
                            return LikeOperator.LikeString(x.Value.Author, args[i], CompareMethod.Binary);
                        });

                        foreach (var item in res)
                            set.Add(item.Key);
                        break;

                    case "-s":
                    case "--saved":
                        if (args.Length <= i + 1)
                        {
                            await RespondError("Failed to parse `--saved`, not enough paramaters.");
                            return;
                        }

                        i++; // incrment the value so it wont be continuously intcrmented in the search.
                        res = cfg.Quotes.Where(x =>
                        {
                            return LikeOperator.LikeString(x.Value.SavedBy, args[i], CompareMethod.Binary);
                        });

                        foreach (var item in res)
                            set.Add(item.Key);
                        break;

                    case "-c":
                    case "--contains":
                        if (args.Length <= i + 1)
                        {
                            await RespondError("Failed to parse `--contains`, not enough paramaters.");
                            return;
                        }

                        i++; // incrment the value so it wont be continuously intcrmented in the search.
                        res = cfg.Quotes.Where(x =>
                        {
                            return LikeOperator.LikeString(x.Value.Content, args[i], CompareMethod.Binary);
                        });

                        foreach (var item in res)
                            set.Add(item.Key);
                        break;
                }

                matches.Add(set);
            }

            HashSet<int> matchingIds = new HashSet<int>();

            for (int i = 0; i < matches.Count; i++)
            {
                if (i == 0)
                    matchingIds = matches[i];
                else
                    matchingIds.IntersectWith(matches[i]);
            }

            string data = "";
            var embedBase = new DiscordEmbedBuilder()
                .WithColor(Color_Cloud)
                .WithTitle($"Quotes Search")
                .AddField("Query:", $"```{string.Join(" ", args)}```");

            foreach (var id in matchingIds)
            {
                var content = cfg.Quotes[id].Content.Length > 53 ? cfg.Quotes[id].Content[0..50] + "..." : cfg.Quotes[id].Content;
                data += $"`{cfg.Quotes[id].Id}`. {content} by `{cfg.Quotes[id].Author}`\n";
            }

            var interact = ctx.Client.GetInteractivity();

            var pages = interact.GeneratePagesInEmbed(data, SplitType.Line, embedBase);

            interact.SendPaginatedMessageAsync(ctx.Channel, ctx.Member, pages, buttons: null);
        }

        private async Task SearchQuotesHelpAsync(InteractionContext ctx)
        {
            var embed = new DiscordEmbedBuilder()
                    .WithColor(Color_Cloud)
                    .WithTitle("Search Quotes Help")
                    .WithDescription("Detailed help for the `/quote search` command.\n" +
                        $"Using `/quote search` without anything else will get you this help command.")
                    .AddField("Full Usage", "```http\n" +
                        $"Usage   ::  /quote search <(-a | --author) <query> | (-s | --saved) <query> | (-c | --content) <query>>\n" +
                        "```")
                    .AddField("`-a | --author <query>`", "```http\n" +
                        $"Usage   ::  /quote search -q \"Author Search\"\n" +
                        $"Usage   ::  /quote search --quote \"Author Search\"\n" +
                        $"Query   :: Search query. Supports wildcards and basic string matching. Use \" around multi-word" +
                        $" queries.\n" +
                        $"Returns :: The edited quote." +
                        $"\n```")
                    .AddField("`-s | --saved <query>`", "```http\n" +
                        $"Usage   ::  /quote search -a \"Saved By Search\"\n" +
                        $"Usage   ::  /quote search --author \"Saved By Search\"\n" +
                        $"Query   :: Search query. Supports wildcards and basic string matching. Use \" around multi-word" +
                        $" queries.\n" +
                        $"Returns :: The edited quote." +
                        $"\n```")
                    .AddField("`-c | --content <query>`", "```http\n" +
                        $"Usage   ::  /quote search -s \"Content Search\"\n" +
                        $"Usage   ::  /quote search --saved \"Content Search\"\n" +
                        $"Query   :: Search query. Supports wildcards and basic string matching. Use \" around multi-word" +
                        $" queries.\n" +
                        $"Returns :: The edited quote." +
                        $"\n```")
                    .AddField("`-h | --help`", "```http\n" +
                        $"Usage   ::  /quote search -h\n" +
                        $"Usage   ::  /quote search --help\n" +
                        $"Returns :: This embed." +
                        $"\n```")
                    .AddField("Query Matching Documentation:", "This command uses the Like Operator for searches.\n" +
                        "For more information about searching with this command, see:\n" +
                        "https://bettersolutions.com/vba/strings-characters/like-operator.htm");

            await ctx.FollowUpAsync(new DiscordFollowupMessageBuilder().AddEmbed(embed));
        }
    }
}
