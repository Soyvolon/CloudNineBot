using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using CloudNine.Core.Configuration;
using CloudNine.Core.Database;
using CloudNine.Core.Http;
using CloudNine.Core.Multisearch;
using CloudNine.Core.Multisearch.Searching;
using CloudNine.Discord.Interactions;

using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;

using Microsoft.Extensions.DependencyInjection;

namespace CloudNine.Discord.Commands.Multiserach
{
    [Group("search")]
    public class MultisearchModule : CommandModule
    {
        protected readonly IServiceProvider _services;
        public MultisearchModule(IServiceProvider services)
        {
            this._services = services;
        }

        [Command("display")]
        [Aliases("results")]
        [Description("Displays the current serach.")]
        public async Task DisplaySerachCommandAsync(CommandContext ctx)
        {
            var interact = _services.GetRequiredService<MultisearchInteractivityService>();

            if (interact.ActiveSearches.TryGetValue(ctx.Member.Id, out var manager))
                await interact.StartSearchDisplay(ctx, manager);
            else
                await RespondError($"You have no active search! Start one with `{ctx.Prefix}search`");
        }

        [Command("details")]
        [Aliases("info")]
        [Description("Get the details of a search result.")]
        public async Task DisplaySearchResultDetailsAsync(CommandContext ctx,
            [Description("Result to display details for.")]
            int result)
        {
            result = result - 1;
            var interact = _services.GetRequiredService<MultisearchInteractivityService>();

            if(interact.ActiveSearches.TryGetValue(ctx.Member.Id, out var manager))
            {
                if (manager.TryGetAllResults(out var fics))
                {
                    if (result < 0 || result >= fics.Count)
                        await RespondError("Requested result does not exsist!");
                    else
                    {
                        var db = _services.GetRequiredService<CloudNineDatabaseModel>();
                        var user = await db.FindAsync<MultisearchUser>(ctx.Member.Id);
                        var guild = await db.FindAsync<DiscordGuildConfiguration>(ctx.Guild.Id);

                        if(guild is null)
                        {
                            guild = new()
                            {
                                Id = ctx.Guild.Id
                            };

                            await db.AddAsync(guild);
                            await db.SaveChangesAsync();
                        }

                        if (user is null)
                        {
                            user = new()
                            {
                                Id = ctx.Member.Id
                            };

                            await db.AddAsync(user);
                            await db.SaveChangesAsync();
                        }

                        var embeds = fics[result].GetDiscordEmbeds(guild.MultisearchConfiguration, user.Options);

                        if (embeds is null)
                            await RespondError("Fanfiction failed to be displayed.");
                        else
                        {
                            foreach (var e in embeds)
                                await ctx.RespondAsync(e);
                        }
                    }
                }
                else
                    await RespondError("Failed to get Fanfic listing from active search.");
            }
            else
            {
                await RespondError($"You have no active search! Start one with `{ctx.Prefix}search`");
            }
        }

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

            var search = _services.GetRequiredService<SearchPraseService>();

            if (searchUser.Options.DefaultSearchOptions.SearchConfiguration is not null)
                search.RegisterDefaults(searchUser.Options.DefaultSearchOptions.SearchConfiguration);

            var res = search.ParseSearch(args);

            if (res.DisplayHelp)
            {
                await DisplayHelp(ctx);
                return;
            }
            else if (res.Errored)
            {
                await RespondError(res.ErrorMessage ?? "An unknown error occoured");
            }

            var startEmbed = new DiscordEmbedBuilder();
            startEmbed.WithDescription("Starting Search...")
                .WithColor(Color_Search);

            await ctx.RespondAsync(embed: startEmbed);

            _  = await searchUser.NewSearch(_services.GetRequiredService<BrowserClient>(), res.SearchBuilder?.Build() ?? new(), res.SearchOptions);

            if (searchUser.Manager is not null)
            {
                var interact = _services.GetRequiredService<MultisearchInteractivityService>();
                interact.ActiveSearches[searchUser.Id] = searchUser.Manager;

                await DisplayResults(ctx);
            }
            else
                await RespondError("No results were found.");
        }

        private async Task DisplayHelp(CommandContext ctx)
        {
            var helpBase = new DiscordEmbedBuilder();
            helpBase.WithColor(Color_Search)
                .WithFooter("Cloud Nine Bot - Fanfiction Multiserach")
                .WithTitle("Fanfic Multisearch Help");

            var helpOne = new DiscordEmbedBuilder(helpBase);
            helpOne
                .WithDescription("The Multiserach module is a set of commands that allow users to " +
                "use a single serach to search multiple Fanfiction websites.\n\n" +
                "**Sites Supported:**\n" +
                "Archive Of Our Own\n" +
                "Fanfiction.net\n" +
                "Wattpad")
                .AddField("`--help | -h`", "Displays this embed.")
            #region Search Fields
                .AddField("Search Fields", "These attributes can be used multiple times to " +
                "set the parameters of your serach. Some of these options can also be set as defaults, see `{ctx.Prefix}search options --help` " +
                "for more information.")
                .AddField("`-t | --title <title>`", "```http\n" +
                $"Usage         :: -t \"Fanfic Title\"\n" +
                $"Usage         :: --title \"Fanfic Title\"\n" +
                $"Title         :: Search by a title. Use quotes \" around multi word titles." +
                "\n```")
                .AddField("`-a | --author <authors>`", "```http\n" +
                $"Usage         :: -a \"Author Search\"\n" +
                $"Usage         :: --author \"Author One,Author Two\"\n" +
                $"Author        :: A list of authors, separated by commas. Use quotes \" around searches with spaces in them. Can be used more than once." +
                "\n```")
                .AddField("`-c | --character <characters>`", "```http\n" +
                $"Usage         :: -c \"Character Name\"\n" +
                $"Usage         :: --character \"Character One,Character Two\"\n" +
                $"Characters    :: A list of characters, separated by commas. Use quotes \" around searches with spaces in them. Can be used more than once" +
                "\n```")
                .AddField("`-r | --relationship <relationships>`", "```http\n" +
                $"Usage         :: -r \"Character One & Character Two\"\n" +
                $"Usage         :: --relationship \"Character One/Character Two\"\n" +
                $"Relationships :: A list of relationships, separated by commas. Use quotes \" around searches with spaces in them. Can be used more than once." +
                "\n```")
                .AddField("`-f | --fandom <fandoms>`", "```http\n" +
                $"Usage         :: -f \"Fandom Name\"\n" +
                $"Usage         :: --fandom \"Fandom One,Fandom Two\"\n" +
                $"Fandoms       :: A list of fandoms, separated by commas. Use quotes \" around searches with spaces in them. Can be used more than once." +
                "\n```")
                .AddField("`-o | --other <tags>`", "```http\n" +
                $"Usage         :: -o \"Other Tag\"\n" +
                $"Usage         :: --other \"Tag 1,Tag 2\"\n" +
                $"Tags          :: A list of other tags, separated by commas. Use quotes \" around searches with spaces in them. Can be used more than once." +
                "\n```")
                .AddField("`-l | --likes <dual integers>`", "```http\n" +
                $"Usage         :: -l 1000-10000\n" +
                $"Usage         :: --likes 1000-0\n" +
                $"Dual Integers :: A pair of integers. See the SPECIAL TYPES section for more information." +
                "\n```")
                .AddField("`-v | --views <dual integers>`", "```http\n" +
                $"Usage         :: -v 1000-10000\n" +
                $"Usage         :: --views 1000-0\n" +
                $"Dual Integers :: A pair of integers. See the SPECIAL TYPES section for more information." +
                "\n```")
                .AddField("`-C | --comments <dual integers>`", "```http\n" +
                $"Usage         :: -C 1000-10000\n" +
                $"Usage         :: --comments 1000-0\n" +
                $"Dual Integers :: A pair of integers. See the SPECIAL TYPES section for more information." +
                "\n```");

            var helpTwo = new DiscordEmbedBuilder(helpBase);
            helpTwo
                .AddField("`-w | --words <dual integers>`", "```http\n" +
                $"Usage         :: -w 1000-10000\n" +
                $"Usage         :: --words 1000-0\n" +
                $"Dual Integers :: A pair of integers. See the SPECIAL TYPES section for more information." +
                "\n```")
                .AddField("`-u | --updated <dual times>`", "```http\n" +
                $"Usage         :: -u \">1 month\"\n" +
                $"Usage         :: --updated \"20-50 days\"\n" +
                $"Dual Times    :: A pair of times, followed by a unit. See the SPECIAL TYPES section for more information." +
                "\n```")
                .AddField("`-p | --published <dual times>`", "```http\n" +
                $"Usage         :: -p \">1 month\"\n" +
                $"Usage         :: --published \"20-50 days\"\n" +
                $"Dual Times    :: A pair of times, followed by a unit. See the SPECIAL TYPES section for more information." +
                "\n```")
                .AddField("`-D | --direction <search direction>`", "```http\n" +
                $"Usage         :: -D ascending\n" +
                $"Usage         :: --direction 0\n" +
                $"Search Dir    :: The order wich to order web results. Either Ascending (1) or Descending (0)." +
                "\n```")
                .AddField("`-s | --searchby <search by>`", "```http\n" +
                $"Usage         :: -s bestmatch\n" +
                $"Usage         :: --searchby 1\n" +
                $"Search By     :: How organize the web results. Must be one of the following: bestmatch (0), likes (1), views (2), updateddate (3)" +
                $" publisheddate (4), or comments (5)." +
                "\n```")
                .AddField("`-R | --raiting <raiting>`", "```http\n" +
                $"Usage         :: -R any\n" +
                $"Usage         :: --raiting 5\n" +
                $"Raiting       :: Filter web results by a raiting. Must be one of the following: any (0), general (1), teen (2), mature (3)," +
                $" explicit (4), notexplicit (5)." +
                "\n```")
                .AddField("`-S | --status <status>`", "```http\n" +
                $"Usage         :: -S any\n" +
                $"Usage         :: --status 2\n" +
                $"Status        :: Filter web results by the fic status. Must be one of the following: any (0), inprogress (1), complete (2)." +
                "\n```")
                .AddField("`-x | --crossover <crossover>`", "```http\n" +
                $"Usage         :: -x any\n" +
                $"Usage         :: --crossover 1\n" +
                $"Status        :: Filter web results by if the fic is a crossover. Must be one of the following: any (0), nocrossover (1), crossover (2)." +
                "\n```")
            #endregion
            #region Search Options
                .AddField("Search Options", "These options override your default options for your account and chnage " +
                $"how the serach will be executed. If you want to change your default options, see `{ctx.Prefix}search options --help`")
                .AddField("`-e | --explicit <true/false>`", "```http\n" +
                $"Usage         :: -e true\n" +
                $"Usage         :: --explicit false\n" +
                $"True/False    :: A boolean value: ture (1) or false (0)" +
                "\n```")
                .AddField("`-W | --warnsingsnotusedaswarnings <true/false>`", "```http\n" +
                $"Usage         :: -W true\n" +
                $"Usage         :: --warnsingsnotusedaswarnings <false>\n" +
                $"True/False    :: A boolean value: ture (1) or false (0)" +
                "\n```")
            #endregion
            #region Special Types
                .AddField("SPECIAL TYPES",
                    "These types are different than the normal options and have a lot of customization in their usage.")
                .AddField("Dual Integers",
                    "This is a pair of integers. A 0 in either slot means no value, not a value of 0. It works as a `min-max` value," +
                    " where a non used value has no effect.\n" +
                    "Examples (for word count):" +
                    "```http\n" +
                    "100-200   :: 100 to 200 words only.\n" +
                    "1000-0    :: Less than or equal to 1000 words only.\n" +
                    "0-1000    :: Greater than or eqaul to 1000 words only." +
                    "\n```")
                .AddField("Dual Times",
                    "This is a pair of times or a less then/greater than a time value. A 0 means no value, not a value of 0. It works as a `min-max` value," +
                    " where a non used value has no effect.\n" +
                    "Examples (for publish date):" +
                    "```http\n" +
                    ">5 days   :: Updated more than 5 days ago.\n" +
                    "0-5 days  :: Updated more than 5 days ago.\n" +
                    "<5 days   :: Updated less than 5 days ago.\n" +
                    "5-0 days  :: Updated less than 5 days ago.\n" +
                    "1-5 days  :: Updated between 1 and 5 days ago." +
                    "\n```\n" +
                    "Furthermore, you can sepcify the amount of weight the numbers have:" +
                    "```http\n" +
                    ">5 days   :: The value 5 is in days.\n" +
                    ">5 weeks  :: The value of 5 is in days times 7 (weeks).\n" +
                    ">5 months :: The value of 5 is in days times 30 (months).\n" +
                    ">5 years  :: The value of 5 is in days times 365 (years).\n" +
                    ">5        :: The value of 5 is in days." +
                    "\n```");
            #endregion

            await ctx.RespondAsync(embed: helpOne);
            await ctx.RespondAsync(embed: helpTwo);
        }

        private async Task DisplayResults(CommandContext ctx)
        {
            var cnext = ctx.Client.GetCommandsNext();
            var cmd = cnext.FindCommand("search display", out var args);
            var fake = cnext.CreateContext(ctx.Message, ctx.Prefix, cmd, args);
            await cnext.ExecuteCommandAsync(fake);
        }
    }
}
