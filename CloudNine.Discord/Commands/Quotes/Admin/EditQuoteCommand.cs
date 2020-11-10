using System.Threading;
using System.Threading.Tasks;

using CloudNine.Core.Configuration;
using CloudNine.Core.Database;

using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;

namespace CloudNine.Discord.Commands.Quotes.Admin
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
            [Description("ID of the quote to edit")]
            int quoteId,
            
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

            if(!cfg.Quotes.TryGetValue(quoteId, out var quote))
            {
                await Respond("Quote not found!");
                return;
            }

            for(int i = 0; i < args.Length; i++)
            {
                switch (args[i].ToLower())
                {
                    case "-a":
                    case "--author":
                        if (args.Length < i + 1)
                            await Respond("Failed to parse `--author`, not enough paramaters.");
                        else
                            quote.Author = args[++i];
                        break;

                    case "-q":
                    case "--quote":
                        if (args.Length < i + 1)
                            await Respond("Failed to parse `--quote`, not enough paramaters.");
                        else
                            quote.Content = args[++i];
                        break;
                }
            }

            await ctx.RespondAsync($"Edited quote `{quoteId}`: ");
            var cnext = ctx.Client.GetCommandsNext();

            var cmd = cnext.FindCommand("quote", out _);

            var fakeCtx = cnext.CreateFakeContext(ctx.Member, ctx.Channel, $"{ctx.Prefix}quote id {quote.Id}", ctx.Prefix, cmd, $"id {quote.Id}");
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
                        "Usage          :: !editquote <quote id> [(-q | --quote) <new quote> | (-a | --author) <new author>]\n" +
                        "Optional Usage :: !editquote 0 help\n" +
                        "```")
                    .AddField("`-q | --quote <new quote>`", "```http\n" +
                        $"Usage        :: {ctx.Prefix}editquote -q \"This is the new Quote\"\n" +
                        $"Usage        :: {ctx.Prefix}editquote --quote \"This is the new Quote\"\n" +
                        $"New Quote    :: Replces the content of the quote with the next argument. Use \" around multi-word" +
                        $" quotes.\n" +
                        $"Returns      :: The edited quote." +
                        $"\n```")
                    .AddField("`-a | --author <new author>`", "```http\n" +
                        $"Usage        :: {ctx.Prefix}editquote -a \"Cloud Bot\"\n" +
                        $"Usage        :: {ctx.Prefix}editquote --author \"Cloud Bot\"\n" +
                        $"New Quote    :: Replces the author of the quote with the next argument. Use \" around multi-word" +
                        $" authors.\n" +
                        $"Returns      :: The edited quote." +
                        $"\n```")
                    .AddField("`-h | --help`", "```http\n" +
                        $"Usage     :: {ctx.Prefix}editquote 0 -h\n" +
                        $"Usage     :: {ctx.Prefix}editquote 0 --help\n" +
                        $"Returns   :: This embed." +
                        $"\n```");

            await ctx.RespondAsync(embed: embed);
        }
    }
}
