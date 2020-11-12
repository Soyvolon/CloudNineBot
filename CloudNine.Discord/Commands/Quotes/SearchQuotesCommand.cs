using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using CloudNine.Core.Configuration;
using CloudNine.Core.Database;

using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity.Enums;
using DSharpPlus.Interactivity.Extensions;

using Microsoft.EntityFrameworkCore.Query.SqlExpressions;
using Microsoft.VisualBasic;
using Microsoft.VisualBasic.CompilerServices;

namespace CloudNine.Discord.Commands.Quotes
{
    public class SearchQuotesCommand : CommandModule
    {
        private readonly CloudNineDatabaseModel _database;

        public SearchQuotesCommand(CloudNineDatabaseModel database)
        {
            this._database = database;
        }

        [Command("searchquotes")]
        [Description("Searches quotes by a specific serach")]
        [Aliases("searchquote", "quoteserach")]
        public async Task SearchQuotesCommandAsync(CommandContext ctx,
            [Description("Quote serach arguments.")]
            params string[] args)
        {
            var cfg = await _database.FindAsync<DiscordGuildConfiguration>(ctx.Guild.Id);
            if(cfg is null)
            {
                var b = ErrorBase().WithDescription("No quotes found.");
                await ctx.RespondAsync(embed: b);
                return;
            }

            if(args.Length <= 0 || args[0] == "-h" || args[0] == "--help")
            {
                await SearchQuotesHelpAsync(ctx);
                return;
            }

            List<HashSet<int>> matches = new List<HashSet<int>>();

            for(int i = 0; i < args.Length; i++)
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
                        if(args.Length <= i + 1)
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

            for(int i = 0; i < matches.Count; i++)
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

            interact.SendPaginatedMessageAsync(ctx.Channel, ctx.Member, pages);
        }

        private async Task SearchQuotesHelpAsync(CommandContext ctx)
        {
            var embed = new DiscordEmbedBuilder()
                    .WithColor(Color_Cloud)
                    .WithTitle("Search Quotes Help")
                    .WithDescription("Detailed help for the `searchquotes` command.\n" +
                        $"Using `{ctx.Prefix}searchquotes` without anything else will get you this help command.")
                    .AddField("Full Usage", "```http\n" +
                        $"Usage   :: {ctx.Prefix}searchquotes <(-a | --author) <query> | (-s | --saved) <query> | (-c | --content) <query>>\n" +
                        "```")
                    .AddField("`-a | --author <query>`", "```http\n" +
                        $"Usage   :: {ctx.Prefix}searchquotes -q \"Author Search\"\n" +
                        $"Usage   :: {ctx.Prefix}searchquotes --quote \"Author Search\"\n" +
                        $"Query   :: Search query. Supports wildcards and basic string matching. Use \" around multi-word" +
                        $" queries.\n" +
                        $"Returns :: The edited quote." +
                        $"\n```")
                    .AddField("`-s | --saved <query>`", "```http\n" +
                        $"Usage   :: {ctx.Prefix}searchquotes -a \"Saved By Search\"\n" +
                        $"Usage   :: {ctx.Prefix}searchquotes --author \"Saved By Search\"\n" +
                        $"Query   :: Search query. Supports wildcards and basic string matching. Use \" around multi-word" +
                        $" queries.\n" +
                        $"Returns :: The edited quote." +
                        $"\n```")
                    .AddField("`-c | --content <query>`", "```http\n" +
                        $"Usage   :: {ctx.Prefix}searchquotes -s \"Content Search\"\n" +
                        $"Usage   :: {ctx.Prefix}searchquotes --saved \"Content Search\"\n" +
                        $"Query   :: Search query. Supports wildcards and basic string matching. Use \" around multi-word" +
                        $" queries.\n" +
                        $"Returns :: The edited quote." +
                        $"\n```")
                    .AddField("`-h | --help`", "```http\n" +
                        $"Usage   :: {ctx.Prefix}searchquotes -h\n" +
                        $"Usage   :: {ctx.Prefix}searchquotes --help\n" +
                        $"Returns :: This embed." +
                        $"\n```")
                    .AddField("Query Matching Documentation:", "This command uses the Like Operator for searches.\n" +
                        "For more information about searching with this command, see:\n" +
                        "https://bettersolutions.com/vba/strings-characters/like-operator.htm");

            await ctx.RespondAsync(embed: embed);
        }
    }
}
