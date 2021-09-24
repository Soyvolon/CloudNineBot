using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using CloudNine.Core.Configuration;
using CloudNine.Core.Database;
using CloudNine.Core.Quotes;
using CloudNine.Discord.Services;

using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using DSharpPlus.SlashCommands.Attributes;

using Microsoft.Extensions.DependencyInjection;

using static CloudNine.Discord.Services.QuoteService;

namespace CloudNine.Discord.Commands.Quotes.Management
{
    [SlashCommandGroup("add", "Command Group for adding things!")]
    public class AddQuoteCommand : ApplicationCommandModule
    {
        private readonly IServiceProvider _services;
        private readonly QuoteService _quotes;

        public AddQuoteCommand(IServiceProvider services, QuoteService quotes)
        {
            this._services = services;
            this._quotes = quotes;
        }

        [SlashCommand("quote", "Adds a quote.")]
        [SlashRequireGuild]
        [SlashRequireUserPermissions(Permissions.ManageMessages)]
        public async Task AddQuoteCommandAsync(InteractionContext ctx,
            [Option("Author", "Author of this quote.")]
            DiscordUser author,

            [Option("Arguments", "Arguments to add. Use --help to view help.")]
            string argsRaw)
        {
            await ctx.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource);

            if (argsRaw.Length <= 0)
            {
                await SendAddQuoteHelp(ctx);
                return;
            }

            List<string> args = argsRaw.Split(' ', StringSplitOptions.RemoveEmptyEntries).ToList();

            args.Insert(0, author.Username);
            args.Insert(0, "-a");

            QuoteData data = new();
            List<string> content = new();

            for (int i = 0; i < args.Count; i++)
            {
                switch (args[i])
                {
                    case "-h":
                    case "--help":
                        await SendAddQuoteHelp(ctx);
                        return;
                }

                bool argRun;
                (i, data, argRun) = await _quotes.ExecuteArgumentChecks(args, i, data, ctx);

                if (i == -1 || data is null) return;

                if (!argRun)
                    content.Add(args[i]);
            }

            Dictionary<string, object> defaults = new()
            {
                { "--time", DateTime.UtcNow },
                { "--saved", ctx.User.Username },
            };

            Quote quote = new();

            var relay = new Relay();

            _quotes.ExecuteParamAssignment("--author", relay, defaults,
                data.Author, d => quote.Author = (string)d,
                    null, null);

            _quotes.ExecuteParamAssignment("--saved", relay, defaults,
                data.SavedBy, d => quote.SavedBy = (string)d,
                    null, null);

            _quotes.ExecuteParamAssignment("--image", relay, defaults,
                data.ImageUrl, d => quote.Attachment = (string)d,
                    null, null);

            _quotes.ExecuteParamAssignment("--color", relay, defaults,
                data.Color, d => quote.Color = (DiscordColor?)d,
                    null, null);

            _quotes.ExecuteParamAssignment("--time", relay, defaults,
                data.Time, d => quote.SavedAt = (DateTime?)d,
                    null, null);

            _quotes.ExecuteParamAssignment("--custom", relay, defaults,
                data.CustomId, d => quote.CustomId = (string?)d,
                    null, null);

            quote.Content = string.Join(" ", content);

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

            _database.Update(cfg);

            await cfg.AddQuote(quote);

            await _database.SaveChangesAsync();

            await ctx.FollowUpAsync(new DiscordFollowupMessageBuilder()
                .AddEmbed(quote.BuildQuote())
                .WithContent($"Added quote: "));
        }

        public async Task SendAddQuoteHelp(InteractionContext ctx)
        {
            var embed = new DiscordEmbedBuilder()
                    .WithColor(CommandModule.Color_Cloud)
                    .WithTitle("Edit Quote Help")
                    .WithDescription("Detailed help for the `/quote add new` command.\n" +
                        $"Using `/quote add new` without anything else will get you this help command.")
                    .AddField("Full Usage",
                        "The add quote command is a variable system that allows for editing as many or as few attributes as needed" +
                        " when creating a new quote." +
                        "```http\n" +
                        $"Usage        :: /editquote <quote> <arguments>\n" +
                        $"Quote        :: Any plain text not attached to an argument becomes the Quotes body.\n" +
                        $"Arguments    :: See the arguments section for a full list.\n" +
                        "```")
                    .AddField("Arguments",
                        "These define what action an edit operation takes.")
                    .AddField("`-a | --author <new author>` OPTIONAL", "```http\n" +
                        $"Usage        :: -a \"Cloud Bot\"\n" +
                        $"Usage        :: --author \"Cloud Bot\"\n" +
                        $"New Author   :: Replaces the author of the quote with the next argument. Use \" around multi-word" +
                        $" authors.\n" +
                        $"\n```")
                    .AddField("`-s | --saved <new saved by message>` OPTIONAL", "```http\n" +
                        $"Usage        :: -s \"Someone saved this.\"\n" +
                        $"Usage        :: --saved \"Someone saved this.\"\n" +
                        $"New message  :: Replaces the saved by message of the quote with the next argument. Use \" around multi-word" +
                        $" messages.\n" +
                        $"\n```")
                    .AddField("`-i | --image [Image URL]` OPTIONAL", "```http\n" +
                        $"Usage        :: -i \"https://source.com/file.png\"\n" +
                        $"Usage        :: --image \"https://source.com/file.png\"\n" +
                        $"Image URL    :: Image URL to use. Or, attach an image to the command message.\n" +
                        $"\n```")
                    .AddField("`-C | --color <color>` OPTIONAL", "```http\n" +
                        $"Usage        :: -c #6fa9de\n" +
                        $"Usage        :: --color \"64, 51, 83\"\n" +
                        $"Color        :: Color to set the embed to. Must be a Hexadecimal value with an optional #, or an RGB value, where each color value" +
                        $" is between 0 and 255 and is spearated by commas.\n" +
                        $"\n```")
                    .AddField("`-t | --time <time>` OPTIONAL", "```http\n" +
                        $"Usage        :: -t 11/03/2020\n" +
                        $"Usage        :: --time \"11/03/2020 15:23\"\n" +
                        $"Time         :: Time to be displayed in the Saved By section.\n" +
                        $"\n```")
                    .AddField("`--custom <custom Id>` OPTIONAL", "```http\n" +
                        $"Usage        :: --custom \"Custom Id\"\n" +
                        $"Time         :: The custom ID used by the quote.\n" +
                        $"\n```")
                    .AddField("`-h | --help`", "```http\n" +
                        $"Usage     :: /quote add new -h\n" +
                        $"Usage     :: /quote add new --help\n" +
                        $"Returns   :: This embed." +
                        $"\n```");

            await ctx.FollowUpAsync(new DiscordFollowupMessageBuilder()
                .AddEmbed(embed));
        }
    }
}
