using System;
using System.Linq;
using System.Threading.Tasks;

using CloudNine.Core.Configuration;
using CloudNine.Core.Database;
using CloudNine.Core.Quotes;

using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity.Extensions;

namespace CloudNine.Discord.Commands.Quotes.Management
{
    public class HideQuoteCommand : CommandModule
    {
        private readonly CloudNineDatabaseModel _database;

        public HideQuoteCommand(CloudNineDatabaseModel database)
        {
            this._database = database;
        }

        [Command("hidequote")]
        [RequireGuild]
        [Description("Adds a new hidden quote, or hides an exsisting one.")]
        [RequireUserPermissions(Permissions.ManageMessages)]
        [Hidden]
        public async Task HideQuoteCommandAsync(CommandContext ctx)
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

            var quote = new Quote();

            var interact = ctx.Client.GetInteractivity();

            var embed = InteractBase()
                .WithTitle("Hidden Quote Creator!")
                .WithDescription("Welcome to the hidden quote creator. This is interactive method of creating hidden quotes.")
                .AddField("Step 1: Quote Author", "Please enter the author of the quote. This can be a random string" +
                " or a Discord Mention.");

            await ctx.RespondAsync(embed: embed);
            var res = await interact.WaitForMessageAsync(x => x.Author == ctx.Message.Author);

            if (res.TimedOut)
            {
                await InteractTimeout("Failed to input an author.");
                return;
            }

            DiscordUser user;
            if (!((user = res.Result.MentionedUsers.FirstOrDefault()) is null))
            {
                quote.Author = user.Username;
            }
            else
            {
                quote.Author = res.Result.Content;
            }

            embed = InteractBase()
                .WithTitle("Author Set!")
                .WithDescription($"The author has been set to:\n{quote.Author}")
                .AddField("Step 2: Quote Content", "Please enter the content of the quote.");

            await ctx.RespondAsync(embed: embed);
            res = await interact.WaitForMessageAsync(x => x.Author == ctx.Message.Author);

            if (res.TimedOut)
            {
                await InteractTimeout("Failed to input quote content.");
                return;
            }

            quote.Content = res.Result.Content;

            embed = InteractBase()
                .WithTitle("Quote Content Set!")
                .WithDescription($"The quote has been set to:\n{quote.Content}")
                .AddField("Step 3: Saved By!", "Please insert who the quote was saved by," +
                " or type skip to save it as yourself.");

            await ctx.RespondAsync(embed: embed);
            res = await interact.WaitForMessageAsync(x => x.Author == ctx.Message.Author);

            if (res.TimedOut)
            {
                await InteractTimeout("Failed to input saved by content.");
                return;
            }

            if (res.Result.Content.Trim().ToLower() == "skip")
            {
                quote.SavedBy = ctx.Member.Username;
            }
            else
            {
                quote.SavedBy = res.Result.Content;
            }

            embed = InteractBase()
                .WithTitle("Quote Saved By Set!")
                .WithDescription($"The quote will now be listed as Saved by: {quote.SavedBy}")
                .AddField("Final Step: Quote ID", "Set the ID for your quote as a string. This can be anything, " +
                "but as its unlisted make sure to remember it!");

            await ctx.RespondAsync(embed: embed);
            bool repeat;
            do
            {
                res = await interact.WaitForMessageAsync(x => x.Author == ctx.Message.Author);

                if (res.TimedOut)
                {
                    await InteractTimeout("Failed to set quote ID.");
                    return;
                }

                repeat = cfg.HiddenQuotes.ContainsKey(res.Result.Content);
                if(repeat)
                {
                    embed = ErrorBase()
                        .WithDescription("Invalid Custom ID: Id Already In Use.");
                    await ctx.RespondAsync(embed: embed);
                }
            }
            while (repeat);

            quote.Id = -1;
            quote.CustomId = res.Result.Content;
            quote.SavedAt = DateTime.UtcNow;

            _database.Update(cfg);

            await cfg.AddQuote(quote);

            await _database.SaveChangesAsync();

            embed = InteractBase()
                .WithTitle("Quote ID Set!")
                .WithDescription($"The quote will now be accesible with:\n" +
                $"`{ctx.Prefix}quote \"{quote.CustomId}\"`");

            await ctx.RespondAsync(embed: embed);

            var cnext = ctx.Client.GetCommandsNext();

            var cmd = cnext.FindCommand("quote", out _);

            var fakeCtx = cnext.CreateFakeContext(ctx.Member, ctx.Channel, $"{ctx.Prefix}quote id \"{quote.CustomId}\"", ctx.Prefix, cmd, $"id \"{quote.CustomId}\"");
            await cnext.ExecuteCommandAsync(fakeCtx);
        }
    }
}
