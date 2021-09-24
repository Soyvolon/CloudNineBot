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

namespace CloudNine.Discord.Commands.Quotes.Management
{
    [SlashCommandGroup("delete", "Delete command group!")]
    public class DeleteQuoteCommand : SlashCommandBase
    {
        private readonly IServiceProvider _services;

        public DeleteQuoteCommand(IServiceProvider services)
        {
            this._services = services;
        }

        [SlashCommand("quote", "Deltes a quote by its ID.")]
        [SlashRequireGuild]
        [SlashRequireUserPermissions(Permissions.ManageMessages)]
        public async Task DeleteQuoteCommandAsync(InteractionContext ctx,
            [Option("ID", "The ID of the quote to remove.")]
            string quoteIdRaw = "")
        {
            await ctx.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource);

            if(!int.TryParse(quoteIdRaw, out var quoteId))
            {
                await DeleteHiddenQuoteCommandAsync(ctx, quoteIdRaw);
                return;
            }

            var _database = _services.GetRequiredService<CloudNineDatabaseModel>();
            var cfg = await _database.FindAsync<DiscordGuildConfiguration>(ctx.Guild.Id);

            if (cfg.TryRemoveQuote(quoteId, out var quote))
            {
                foreach(var user in cfg.FavoriteQuotes)
                    user.Value.Remove(quoteId); // remove this from the favorites list.

                _database.Update(cfg);
                await _database.SaveChangesAsync();

                var embed = new DiscordEmbedBuilder()
                    .WithTitle($"Removed Quote `{quoteId}` by {quote.Author}")
                    .WithDescription(quote.Content);

                await ctx.FollowUpAsync(new DiscordFollowupMessageBuilder().AddEmbed(embed));
            }
            else
            {
                await Respond($"Failed to find a quote with the ID: `{quoteId}`");
            }
        }

        public async Task DeleteHiddenQuoteCommandAsync(InteractionContext ctx,
            string quoteId)
        {
            var _database = _services.GetRequiredService<CloudNineDatabaseModel>();
            var cfg = await _database.FindAsync<DiscordGuildConfiguration>(ctx.Guild.Id);

            if (cfg.TryRemoveQuote(quoteId, out var quote))
            {
                _database.Update(cfg);
                await _database.SaveChangesAsync();

                var embed = new DiscordEmbedBuilder()
                    .WithTitle($"Removed Quote `{quoteId}` by {quote.Author}")
                    .WithDescription(quote.Content);

                await ctx.FollowUpAsync(new DiscordFollowupMessageBuilder().AddEmbed(embed));
            }
            else
            {
                await Respond($"Failed to find a quote with the ID: `{quoteId}`");
            }
        }
    }
}
