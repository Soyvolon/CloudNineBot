using System;
using System.Threading.Tasks;

using CloudNine.Core.Configuration;
using CloudNine.Core.Database;
using CloudNine.Core.Quotes;

using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;

namespace CloudNine.Discord.Commands.Quotes.Admin
{
    public class AddQuoteCommand : CommandModule
    {
        private readonly CloudNineDatabaseModel _database;

        public AddQuoteCommand(CloudNineDatabaseModel database)
        {
            this._database = database;
        }

        [Command("addquote")]
        [Description("Adds a quote.")]
        [Aliases("aquote")]
        [RequireUserPermissions(Permissions.ManageMessages)]
        public async Task AddQuoteCommandAsync(CommandContext ctx,
            [Description("Quote to add")]
            [RemainingText]
            string quote)
        => await AddQuoteCommandAsync(ctx, null, quote);

        [Command("addquote")]
        [Priority(2)]
        public async Task AddQuoteCommandAsync(CommandContext ctx,
            [Description("Author of the quote.")]
            DiscordMember? discordMember,

            [Description("Quote to add")]
            [RemainingText]
            string quote)
        {
            var cfg = await _database.FindAsync<DiscordGuildConfiguration>(ctx.Guild.Id);
            if (cfg is null)
            {
                cfg = new DiscordGuildConfiguration()
                {
                    Id = ctx.Guild.Id,
                    Prefix = ctx.Prefix,
                };
                await _database.AddAsync(cfg);
                await _database.SaveChangesAsync();
            }

            var quoteObject = new Quote()
            {
                Content = quote,
                Author = discordMember?.Username ?? "unkown",
                SavedAt = DateTime.UtcNow,
                SavedBy = ctx.Member.Username,
            };

            _database.Update(cfg);

            await cfg.AddQuote(quoteObject);

            await _database.SaveChangesAsync();

            await ctx.RespondAsync("Added a new quote: ");
            var cnext = ctx.Client.GetCommandsNext();

            var cmd = cnext.FindCommand("quote", out _);

            var fakeCtx = cnext.CreateFakeContext(ctx.Member, ctx.Channel, $"{ctx.Prefix}quote id {quoteObject.Id}", ctx.Prefix, cmd, $"id {quoteObject.Id}");
            await cnext.ExecuteCommandAsync(fakeCtx);
        }
    }
}
