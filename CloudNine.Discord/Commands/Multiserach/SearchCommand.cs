using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

using CloudNine.Core.Database;
using CloudNine.Core.Http;
using CloudNine.Core.Multisearch;
using CloudNine.Core.Multisearch.Builders;
using CloudNine.Core.Multisearch.Configuration;

using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;

using Microsoft.Extensions.DependencyInjection;

namespace CloudNine.Discord.Commands.Multiserach
{
    public class SearchCommand : MultisearchCommandBase
    {
        public SearchCommand(IServiceProvider s) : base(s) { }

        [GroupCommand]
        [Description("Search for a fanfic.")]
        public async Task SearchCommandAsync(CommandContext ctx, 
            [Description("Search arguments.")]
            params string[] args)
        {
            var db = _services.GetRequiredService<CloudNineDatabaseModel>();
            var searchUser = await db.FindAsync<MultisearchUser>(ctx.Member.Id);

            if (searchUser is null)
            {
                searchUser = new()
                {
                    Id = ctx.Member.Id
                };

                await db.AddAsync(searchUser);
                await db.SaveChangesAsync();
            }

            var startEmbed = new DiscordEmbedBuilder();
            startEmbed.WithDescription("Starting Search...")
                .WithColor(Color_Search);

            await ctx.RespondAsync(embed: startEmbed);

            var search = ParseSearchArguments(out var help, out var options, args);

            if(help || search is null)
            {
                await DisplayHelp(ctx);
                return;
            }

            var fanfics = await searchUser.NewSearch(_services.GetRequiredService<BrowserClient>(), search.Build(), options);


        }

        private async Task DisplayHelp(CommandContext ctx)
        {
            var helpEmbed = new DiscordEmbedBuilder();
            helpEmbed.WithTitle("Fanfic Multisearch Help")
                .WithDescription("The Multiserach module is a set of commands that allow users to " +
                "use a single serach to search multiple Fanfiction websites.\n\n" +
                "**Sites Supported:**\n" +
                "Archive Of Our Own\n" +
                "Fanfiction.net\n" +
                "Wattpad")
                .AddField("Search Fields`", "These attributes can be used multiple times to " +
                "set the parameters of your serach")
                .AddField("Search Options", "These options override your default options for your account and chnage " +
                $"how the serach will be executed. If you want to change your default options, see `{ctx.Prefix}search options --help`");

            await ctx.RespondAsync(embed: helpEmbed);
        }

        private static SearchBuilder? ParseSearchArguments(out bool displayHelp, out SearchOptions? options, params string[] args)
        {
            var builder = new SearchBuilder();
            options = null; // this will be created later, if a config option is needed.
            displayHelp = false;

            foreach(var a in args)
            {
                switch(a.Trim().ToLower())
                {
                    case "--help":
                    case "-h":
                        // Display the help information
                        displayHelp = true;
                        return null;
                }
            }

            return builder;
        }
    }
}
