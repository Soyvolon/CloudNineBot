using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using CloudNine.Core.Configuration;
using CloudNine.Core.Database;
using CloudNine.Core.Quotes;
using CloudNine.Discord.Services;

using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;

using static CloudNine.Discord.Services.QuoteService;

namespace CloudNine.Discord.Commands.Quotes.Management
{
    public class EditQuoteCommand : CommandModule
    {
        private readonly CloudNineDatabaseModel _database;
        private readonly QuoteService _quotes;

        public EditQuoteCommand(CloudNineDatabaseModel database, QuoteService quotes)
        {
            this._database = database;
            this._quotes = quotes;
        }

        [Command("editquote")]
        [RequireGuild]
        [Description("Edits an exsisting quote.")]
        [RequireUserPermissions(Permissions.ManageMessages)]
        [Priority(2)]
        public async Task EditQuoteCommandAsync(CommandContext ctx,
            
            [Description("Arguments for editing the command. Use `-h | --help` for more information.")]
            params string[] args)
        {
            if(args.Length <= 0 || args[0] == "-h" || args[0] == "--help")
            { // Show help information.
                await EditQuoteHelpCommandAsync(ctx);
                return;
            }

            var cfg = await _database.FindAsync<DiscordGuildConfiguration>(ctx.Guild.Id);

            if(cfg is null)
            {
                await Respond("Quote not found!");
                return;
            }

            Quote quote;
            bool hidden = false;
            if(int.TryParse(args[0], out int id))
            {
                if (!cfg.Quotes.TryGetValue(id, out quote))
                {
                    await Respond("Quote not found!");
                    return;
                }
            }
            else
            {
                if(!cfg.HiddenQuotes.TryGetValue(args[0], out quote))
                {
                    await Respond("Quote not found!");
                    return;
                }

                hidden = true;
            }

            _database.Update(cfg);

            var data = new QuoteData();
            var argsList = args[1..].ToList();

            for(int i = 0; i < argsList.Count; i++)
            {
                (i, data, _) = await _quotes.ExecuteArgumentChecks(argsList, i, data, ctx.Message);

                if (i == -1 || data is null) return;
            }

            var defaults = new Dictionary<string, object>()
            {
                { "--author", quote.Author },
                { "--saved", quote.SavedBy },
                { "--image", quote.Attachment },
                { "--color", quote.Color },
                { "--time", quote.SavedAt },
                { "--custom", quote.CustomId },
                { "--message", quote.Content }
            };

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

            _quotes.ExecuteParamAssignment("--message", relay, defaults,
                data.Content, d => quote.Content = (string?)d,
                    null, null);

            await _database.SaveChangesAsync();

            string cmdFull;
            string cmdArgs;
            if(hidden)
            {
                cmdFull = $"{ctx.Prefix}quote id \"{quote.CustomId}\"";
                cmdArgs = $"id \"{quote.CustomId}\"";
            }
            else
            {
                cmdFull = $"{ctx.Prefix}quote id {quote.Id}";
                cmdArgs = $"id {quote.Id}";
            }

            await ctx.RespondAsync($"Edited quote `{args[0]}`: ");
            var cnext = ctx.Client.GetCommandsNext();

            var cmd = cnext.FindCommand("quote", out _);

            var fakeCtx = cnext.CreateFakeContext(ctx.Member, ctx.Channel, cmdFull, ctx.Prefix, cmd, cmdArgs);
            await cnext.ExecuteCommandAsync(fakeCtx);
        }

        [Command("editquote")]
        public async Task EditQuoteHelpCommandAsync(CommandContext ctx)
        {
            var embed = new DiscordEmbedBuilder()
                    .WithColor(Color_Cloud)
                    .WithTitle("Edit Quote Help")
                    .WithDescription("Detailed help for the `editquote` command.\n" +
                        $"Using `{ctx.Prefix}editquote` without anything else will get you this help command.")
                    .AddField("Full Usage", 
                        "The edit command is a variable system that allows for editing as many or as few attributes as needed." +
                        "```http\n" +
                        $"Usage          :: {ctx.Prefix}editquote <arguments>\n" +
                        $"Arguments      :: See the arguments section for a full list.\n" +
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
                    .AddField("`-m | --message <message>` OPTIONAL", "```http\n" +
                        $"Usage        :: -m Words\n" +
                        $"Usage        :: --message \"Lots of words.\"\n" +
                        $"Time         :: Modifies the content of the quote.\n" +
                        $"\n```")
                    .AddField("`-h | --help`", "```http\n" +
                        $"Usage     :: {ctx.Prefix}editquote -h\n" +
                        $"Usage     :: {ctx.Prefix}editquote --help\n" +
                        $"Returns   :: This embed." +
                        $"\n```");

            await ctx.RespondAsync(embed: embed);
        }
    }
}
