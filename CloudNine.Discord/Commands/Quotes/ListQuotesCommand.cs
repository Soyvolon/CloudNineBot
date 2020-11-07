using System.Collections.Immutable;
using System.Threading.Tasks;

using CloudNine.Core.Configuration;
using CloudNine.Core.Database;
using CloudNine.Core.Quotes;

using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity.Extensions;

namespace CloudNine.Discord.Commands.Quotes
{
    public class ListQuotesCommand : CommandModule
    {
        private readonly CloudNineDatabaseModel _database;

        public ListQuotesCommand(CloudNineDatabaseModel database)
        {
            this._database = database;
        }

        [Command("listquotes")]
        [Description("Lists all quotes on this server.")]
        [Aliases("lquotes")]
        public async Task ListQuotesCommandAsync(CommandContext ctx)
        {
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

            string data = "";
            var embedBase = new DiscordEmbedBuilder()
                .WithColor(Color_Cloud)
                .WithTitle($"Quotes saved on {ctx.Guild.Name}");

            var quoteList = cfg.Quotes.Values.ToImmutableSortedSet(new QuoteComparer());

            foreach (var quote in quoteList)
            {
                var content = quote.Content.Length > 53 ? quote.Content[0..50] + "..." : quote.Content;
                data += $"`{quote.Id}`. {content} by `{quote.Author}`\n";
            }

            var interact = ctx.Client.GetInteractivity();

            var pages = interact.GeneratePagesInEmbed(data, DSharpPlus.Interactivity.Enums.SplitType.Line, embedBase);

            interact.SendPaginatedMessageAsync(ctx.Channel, ctx.Member, pages);
        }
    }
}
