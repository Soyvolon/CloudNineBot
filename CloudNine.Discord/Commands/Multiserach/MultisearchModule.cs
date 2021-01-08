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
            var helpEmbed = new DiscordEmbedBuilder();
            helpEmbed.WithTitle("Fanfic Multisearch Help")
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
                .AddField("`--title | -t", "```http\n" +
                "" +
                "\n```")
                .AddField("`--title | -t", "```http\n" +
                "" +
                "\n```")
                .AddField("`--title | -t", "```http\n" +
                "" +
                "\n```")
                .AddField("`--title | -t", "```http\n" +
                "" +
                "\n```")
                .AddField("`--title | -t", "```http\n" +
                "" +
                "\n```")
                .AddField("`--title | -t", "```http\n" +
                "" +
                "\n```")
                .AddField("`--title | -t", "```http\n" +
                "" +
                "\n```")
                .AddField("`--title | -t", "```http\n" +
                "" +
                "\n```")
                .AddField("`--title | -t", "```http\n" +
                "" +
                "\n```")
                .AddField("`--title | -t", "```http\n" +
                "" +
                "\n```")
                .AddField("`--title | -t", "```http\n" +
                "" +
                "\n```")
                .AddField("`--title | -t", "```http\n" +
                "" +
                "\n```")
                .AddField("`--title | -t", "```http\n" +
                "" +
                "\n```")
                .AddField("`--title | -t", "```http\n" +
                "" +
                "\n```")
                .AddField("`--title | -t", "```http\n" +
                "" +
                "\n```")
                .AddField("`--title | -t", "```http\n" +
                "" +
                "\n```")
                .AddField("`--title | -t", "```http\n" +
                "" +
                "\n```")
            #endregion
            #region Search Options
                .AddField("Search Options", "These options override your default options for your account and chnage " +
                $"how the serach will be executed. If you want to change your default options, see `{ctx.Prefix}search options --help`")
                .AddField("`--title | -t", "```http\n" +
                "" +
                "\n```")
                .AddField("`--title | -t", "```http\n" +
                "" +
                "\n```")
            #endregion
                .WithColor(Color_Search)
                .WithFooter("Cloud Nine Bot - Fanfiction Multiserach");

            await ctx.RespondAsync(embed: helpEmbed);
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
