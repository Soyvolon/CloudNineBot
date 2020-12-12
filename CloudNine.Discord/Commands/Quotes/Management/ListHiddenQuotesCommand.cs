using System;
using System.Collections.Immutable;
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

namespace CloudNine.Discord.Commands.Quotes.Management
{
    public class ListHiddenQuotesCommand : CommandModule
    {
        private readonly IServiceProvider _services;

        public ListHiddenQuotesCommand(IServiceProvider services)
        {
            this._services = services;
        }

        [Command("listhiddenquotes")]
        [RequireGuild]
        [Description("Lists the hidden quotes.")]
        [Aliases("lhquotes")]
        [RequireUserPermissions(Permissions.ManageMessages)]
        [Hidden]
        public async Task ListHiddenQuotesCommandAsync(CommandContext ctx)
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

            var interact = ctx.Client.GetInteractivity();

            var pages = interact.GeneratePagesInEmbed(data, DSharpPlus.Interactivity.Enums.SplitType.Line, embedBase);

            interact.SendPaginatedMessageAsync(ctx.Channel, ctx.Member, pages);
        }
    }
}
