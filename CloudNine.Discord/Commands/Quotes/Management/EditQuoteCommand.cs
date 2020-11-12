using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using CloudNine.Core.Configuration;
using CloudNine.Core.Database;
using CloudNine.Core.Quotes;

using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;

namespace CloudNine.Discord.Commands.Quotes.Management
{
    public class EditQuoteCommand : CommandModule
    {
        private readonly CloudNineDatabaseModel _database;

        public EditQuoteCommand(CloudNineDatabaseModel database)
        {
            this._database = database;
        }

        [Command("editquote")]
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

            for(int i = 1; i < args.Length; i++)
            {
                switch (args[i].ToLower())
                {
                    case "-a":
                    case "--author":
                        if (args.Length <= i + 1)
                            await RespondError("Failed to parse `--author`, not enough paramaters.");
                        else
                            quote.Author = args[++i];
                        break;

                    case "-q":
                    case "--quote":
                        if (args.Length <= i + 1)
                            await RespondError("Failed to parse `--quote`, not enough paramaters.");
                        else
                            quote.Content = args[++i];
                        break;

                    case "-s":
                    case "--saved":
                        if (args.Length <= i + 1)
                            await RespondError("Failed to parse `--saved`, not enough paramaters.");
                        else
                            quote.SavedBy = args[++i];
                        break;

                    case "-c":
                    case "--custom":
                        if (args.Length <= i + 1)
                            await RespondError("Failed to parse `--custom`, not enough paramaters.");
                        else
                        {
                            if (cfg.HiddenQuotes.TryRemove(args[0], out _))
                                cfg.HiddenQuotes[args[i + 1]] = quote;

                            quote.CustomId = args[++i];
                        }
                        break;
                    case "-i":
                    case "--image":
                        string url = "";
                        if (args.Length <= i + 1)
                        {
                            if (ctx.Message.Attachments.Count > 0)
                                url = ctx.Message.Attachments.First().Url;

                            await RespondError("Failed to parse `--image`, not enough paramaters, and/or image not attached.");
                        }
                        else
                        {
                            try
                            {
                                var uri = new Uri(args[++i]);
                                url = uri.AbsoluteUri;
                            }
                            catch { } // just leave url as "", it will be skipped later.

                        }

                        if(url != "")
                        {
                            quote.Attachment = url;
                        }
                        else
                        {
                            await RespondError("Failed to get a valid URL.");
                        }
                        break;
                }
            }

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
                    .AddField("Full Usage", "```http\n" +
                        $"Usage          :: {ctx.Prefix}editquote <quote id> [(-q | --quote) <new quote> | (-a | --author) <new author> | " +
                        "(-s | --saved) <new saved by message>]\n" +
                        $"Optional Usage :: {ctx.Prefix}editquote --help\n" +
                        "```")
                    .AddField("`-q | --quote <new quote>`", "```http\n" +
                        $"Usage        :: {ctx.Prefix}editquote -q \"This is the new Quote\"\n" +
                        $"Usage        :: {ctx.Prefix}editquote --quote \"This is the new Quote\"\n" +
                        $"New Quote    :: Replaces the content of the quote with the next argument. Use \" around multi-word" +
                        $" quotes.\n" +
                        $"Returns      :: The edited quote." +
                        $"\n```")
                    .AddField("`-a | --author <new author>`", "```http\n" +
                        $"Usage        :: {ctx.Prefix}editquote -a \"Cloud Bot\"\n" +
                        $"Usage        :: {ctx.Prefix}editquote --author \"Cloud Bot\"\n" +
                        $"New Author   :: Replaces the author of the quote with the next argument. Use \" around multi-word" +
                        $" authors.\n" +
                        $"Returns      :: The edited quote." +
                        $"\n```")
                    .AddField("`-s | --saved <new saved by message>`", "```http\n" +
                        $"Usage        :: {ctx.Prefix}editquote -s \"Someone saved this.\"\n" +
                        $"Usage        :: {ctx.Prefix}editquote --saved \"Someone saved this.\"\n" +
                        $"New message  :: Replaces the saved by message of the quote with the next argument. Use \" around multi-word" +
                        $" messages.\n" +
                        $"Returns      :: The edited quote." +
                        $"\n```")
                    .AddField("`-c | --custom <new custom ID>`", "```http\n" +
                        $"Usage        :: {ctx.Prefix}editquote -c \"Quote One\"\n" +
                        $"Usage        :: {ctx.Prefix}editquote --custom \"Quote One\"\n" +
                        $"New message  :: Sets the custom ID for a quote. Use \" around multi-word" +
                        $" quotes.\n" +
                        $"Returns      :: The edited quote." +
                        $"\n```")
                    .AddField("`-i | --image [Image URL]`", "```http\n" +
                        $"Usage        :: {ctx.Prefix}editquote -i \"https://source.com/file.png\"\n" +
                        $"Usage        :: {ctx.Prefix}editquote --image \"https://source.com/file.png\"\n" +
                        $"Image URL    :: Image URL to use. Or, attach an image to the command message." +
                        $"Returns      :: The edited quote." +
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
