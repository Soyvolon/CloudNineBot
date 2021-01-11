using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using CloudNine.Core.Configuration;
using CloudNine.Core.Database;
using CloudNine.Core.Http;
using CloudNine.Core.Multisearch;
using CloudNine.Core.Multisearch.Configuration;
using CloudNine.Core.Multisearch.Searching;
using CloudNine.Discord.Interactions;

using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity.Enums;
using DSharpPlus.Interactivity.Extensions;

using Microsoft.Extensions.DependencyInjection;

namespace CloudNine.Discord.Commands.Multiserach
{
    [Group("search")]
    [Description("Commands for the Fanfiction Multisearch module.")]
    public partial class MultisearchModule : CommandModule
    {
        protected readonly IServiceProvider _services;
        public MultisearchModule(IServiceProvider services)
        {
            this._services = services;
        }

        [Group("config")]
        [Description("Commands for configuring how Cloud Bot searches and parses the search information.")]
        public class MultisearchSettings : CommandModule
        {
            protected readonly string[] NotAllowedInGuild = new string[] { "direction", "searchby", "rating", "complete", "crossover" };
            protected readonly string[] AvalibleOptions = new string[] { "overflow", "hidesensitive", "taglimit", "ctaglimit", "rtaglimit",
                "cache", "link", "explicit", "warnonnowarn", "direction", "searchby", "rating", "complete", "crossover" };
            protected readonly IServiceProvider _services;
            public MultisearchSettings(IServiceProvider services)
            {
                this._services = services;
            }

            [GroupCommand]
            [Description("Shows help for configuration options.")]
            public async Task SearchConfigHelp(CommandContext ctx)
            {
                var builder = new DiscordEmbedBuilder();
                builder.WithTitle("Multisearch Config Help")
                    .AddField("Avalible Options",
                    $"`{string.Join("`, `", AvalibleOptions)}`")
                    .AddField("Basic Value Types",
                    "**True/False** options take boolean values. Either True, False, 1, or 0 must be entered.\n\n" +
                    "**Numerical** options take a number. This must be an interger value. Anything 0 or below will be uased as no limit.")
                    .AddField("Special Value Types",
                    "`direction` requires one of the following (or the number): ```ascending (1), descending (0).```\n" +
                    "`searchby` requires one of the following (or the number): ```bestmatch (0), likes (1), views (2), updateddate (3)" +
                    $" publisheddate (4), or comments (5).```\n" +
                    "`rating` requires one of the following (or the number): ```any (0), general (1), teen (2), mature (3)," +
                $" explicit (4), notexplicit (5).```\n" +
                    "`complete` requires one of the following (or the number): ```any (0), inprogress (1), complete (2).```\n" +
                    "`crossover` requires one of the following (or the number): ```any (0), nocrossover (1), crossover (2).```")
                    .AddField("Want to view the current Configs?",
                    $"`{ctx.Prefix}search config user` to view your personal config.\n" +
                    $"`{ctx.Prefix}search config guild` to view this servers config.")
                    .WithColor(Color_Search);

                await ctx.RespondAsync(builder);
            }

            [Command("user")]
            [Aliases("member")]
            [Description("Set or view personal settings.")]
            public async Task UserConfigCommandAsync(CommandContext ctx,
                [Description("Setting to change. Leave blank to view current settings.")]
                string? setting = null,
                
                [Description("Value to change it to. Leave blank to set it back to the defualt.")]
                string? value = null)
            {
                if(setting is null)
                {
                    await DisplayUserSettings(ctx);
                }
                else
                {
                    await UpdateOption(ctx, setting, value, false);
                }
            }

            private async Task DisplayUserSettings(CommandContext ctx)
            {
                var db = _services.GetRequiredService<CloudNineDatabaseModel>();
                var user = await db.FindAsync<MultisearchUser>(ctx.Member.Id);

                if (user is null)
                {
                    user = new()
                    {
                        Id = ctx.Member.Id
                    };

                    await db.AddAsync(user);
                    await db.SaveChangesAsync();
                }

                var settings = user.Options;

                var builder = GetSharedEmbed(settings);
                builder.AddField("Search Configuration",
                    $"Search Direction (`direction`): **{settings.DefaultSearchOptions?.SearchConfiguration?.Direction.GetString() ?? "Not Specified"}**\n\n" +
                    $"Search By (`searchby`): **{settings.DefaultSearchOptions?.SearchConfiguration?.SearchFicsBy.GetString() ?? "Not Specified"}**\n\n" +
                    $"Rating (`rating`): **{settings.DefaultSearchOptions?.SearchConfiguration?.FicRating.GetString() ?? "Not Specified"}**\n\n" +
                    $"Completion Status (`complete`): **{settings.DefaultSearchOptions?.SearchConfiguration?.Status.GetString() ?? "Not Specified"}**\n\n" +
                    $"Crossover Status (`crossover`): **{settings.DefaultSearchOptions?.SearchConfiguration?.Crossover.GetString() ?? "Not Specified"}**")
                    .WithTitle("User Configuration")
                    .WithAuthor(ctx.Member.DisplayName, ctx.Member.AvatarUrl, ctx.Member.AvatarUrl)
                    .WithColor(Color_Search)
                    .WithDescription("Global settings for you. These will be overwritten by a guild setting if it is lower. For example," +
                    " if you allow explicit content, but the guild does not, then no explicit content will be show.\n\n" +
                    "**Values in the parenthesises `( )` are the config option name. Use this name when setting a config value.**");

                await ctx.RespondAsync(builder);
            }

            [Command("guild")]
            [Aliases("server")]
            [Description("Set or view server settings.")]
            public async Task GuildConfigCommandAsync(CommandContext ctx,
                [Description("Setting to change. Leave blank to view current settings.")]
                string? setting = null,

                [Description("Value to change it to. Leave blank to set it back to the defualt.")]
                string? value = null)
            {
                if(setting is null)
                {
                    await DisplayGuildSettings(ctx);
                }
                else
                {
                    if(!ctx.Member.Guild.Permissions?.HasPermission(Permissions.ManageMessages) ?? false)
                    {
                        await RespondError("You do not have permissions to modify the servers configuration!");
                        return;
                    }

                    if(value is not null && NotAllowedInGuild.Contains(value))
                    {
                        await RespondError($"The {value} option is for user configurations only.");
                    }
                    else
                    {
                        await UpdateOption(ctx, setting, value, true);
                    }
                }
            }

            private async Task DisplayGuildSettings(CommandContext ctx)
            {
                var db = _services.GetRequiredService<CloudNineDatabaseModel>();
                var guild = await db.FindAsync<DiscordGuildConfiguration>(ctx.Guild.Id);

                if (guild is null)
                {
                    guild = new()
                    {
                        Id = ctx.Member.Id
                    };

                    await db.AddAsync(guild);
                    await db.SaveChangesAsync();
                }

                var builder = GetSharedEmbed(guild.MultisearchConfiguration);
                builder.WithTitle($"Server Configuration")
                    .WithAuthor(ctx.Guild.Name, ctx.Guild.IconUrl, ctx.Guild.IconUrl)
                    .WithColor(Color_Search)
                    .WithDescription("Configuration options for this server. Options set here will override user options if they are \"lower\"." +
                    " For example, if a user has explicit content enabled, but the server has it disabled, no explicit content will be displayed.\n\n" +
                    "**Values in the parenthesises `( )` are the config option name. Use this name when setting a config value.**");

                await ctx.RespondAsync(builder);
            }

            private DiscordEmbedBuilder GetSharedEmbed(MultisearchConfigurationOptions options)
            {
                var builder = new DiscordEmbedBuilder();
                builder.AddField("Configuration Options",
                    $"Overflow Description (`overflow`): **{options.OverflowDescription}**\n\n" +
                    $"Hide Sensitive Content Descriptions (`hidesensitive`): **{options.HideSensitiveContentDescriptions}**\n\n" +
                    $"Other Tag Limit (`taglimit`): **{options.TagLimit}**\n\n" +
                    $"Character Tag Limit (`ctaglimit`): **{options.CharacterTagLimit}**\n\n" +
                    $"Relationship Tag Limit (`rtaglimit`): **{options.RelationshipTagLimit}**\n\n" +
                    $"Cache Fanfics (`cache`): **{options.CacheFanfics}**\n\n" +
                    $"Display Fanfic Link Data (`link`): **{options.DisplayLinkData}**")
                    .AddField("Search Options",
                    $"Allow Explicit (`explicit`): **{options.DefaultSearchOptions.AllowExplicit}**\n\n" +
                    $"Treat Warnings Not Used as Warnings (`warnonnowarn`): **{options.DefaultSearchOptions.TreatWarningsNotUsedAsWarnings}**\n\n");

                return builder;
            }

            private async Task UpdateOption(CommandContext ctx, string option, string? value = null, bool guild = false)
            {
                var defaults = new MultisearchConfigurationOptions();

                var db = _services.GetRequiredService<CloudNineDatabaseModel>();
                DiscordGuildConfiguration? gcfg = null;
                MultisearchUser? user = null;
                MultisearchConfigurationOptions options;
                if(guild)
                {
                    gcfg = await db.FindAsync<DiscordGuildConfiguration>(ctx.Guild.Id);

                    if (gcfg is null)
                    {
                        gcfg = new()
                        {
                            Id = ctx.Guild.Id
                        };

                        await db.AddAsync(gcfg);
                        await db.SaveChangesAsync();
                    }

                    options = gcfg.MultisearchConfiguration;
                }
                else
                {
                    user = await db.FindAsync<MultisearchUser>(ctx.Member.Id);

                    if (user is null)
                    {
                        user = new()
                        {
                            Id = ctx.Member.Id
                        };

                        await db.AddAsync(user);
                        await db.SaveChangesAsync();
                    }

                    options = user.Options;
                }

                string outVal = "";
                SearchParseResult? error = null;
                switch(option.ToLower())
                {
                    case "overflow":
                        if(value is null)
                        {
                            options.OverflowDescription = defaults.OverflowDescription;
                        }
                        else
                        {
                            if((error = SearchParseSevice.BooleanArgument(value, out var res)) is null)
                            {
                                options.OverflowDescription = res;
                            }
                        }

                        outVal = options.OverflowDescription.ToString();
                        break;
                    case "hidesensitive":
                        if (value is null)
                        {
                            options.HideSensitiveContentDescriptions = defaults.HideSensitiveContentDescriptions;
                        }
                        else
                        {
                            if ((error = SearchParseSevice.BooleanArgument(value, out var res)) is null)
                            {
                                options.HideSensitiveContentDescriptions = res;
                            }
                        }

                        outVal = options.HideSensitiveContentDescriptions.ToString();
                        break;
                    case "taglimit":
                        if (value is null)
                        {
                            options.TagLimit = defaults.TagLimit;
                        }
                        else
                        {
                            if ((error = SearchParseSevice.IntegerArgument(value, out var res)) is null)
                            {
                                options.TagLimit = res;
                            }
                        }

                        outVal = options.TagLimit.ToString();
                        break;
                    case "ctaglimit":
                        if (value is null)
                        {
                            options.CharacterTagLimit = defaults.CharacterTagLimit;
                        }
                        else
                        {
                            if ((error = SearchParseSevice.IntegerArgument(value, out var res)) is null)
                            {
                                options.CharacterTagLimit = res;
                            }
                        }

                        outVal = options.CharacterTagLimit.ToString();
                        break;
                    case "rtaglimit":
                        if (value is null)
                        {
                            options.RelationshipTagLimit = defaults.RelationshipTagLimit;
                        }
                        else
                        {
                            if ((error = SearchParseSevice.IntegerArgument(value, out var res)) is null)
                            {
                                options.RelationshipTagLimit = res;
                            }
                        }

                        outVal = options.RelationshipTagLimit.ToString();
                        break;
                    case "explicit":
                        if (value is null)
                        {
                            options.DefaultSearchOptions.AllowExplicit = defaults.DefaultSearchOptions.AllowExplicit;
                        }
                        else
                        {
                            if ((error = SearchParseSevice.BooleanArgument(value, out var res)) is null)
                            {
                                options.DefaultSearchOptions.AllowExplicit = res;
                            }
                        }

                        outVal = options.DefaultSearchOptions.AllowExplicit.ToString();
                        break;
                    case "warnonnowarn":
                        if (value is null)
                        {
                            options.DefaultSearchOptions.TreatWarningsNotUsedAsWarnings = defaults.DefaultSearchOptions.TreatWarningsNotUsedAsWarnings;
                        }
                        else
                        {
                            if ((error = SearchParseSevice.BooleanArgument(value, out var res)) is null)
                            {
                                options.DefaultSearchOptions.TreatWarningsNotUsedAsWarnings = res;
                            }
                        }

                        outVal = options.DefaultSearchOptions.TreatWarningsNotUsedAsWarnings.ToString();
                        break;
                    case "direction":
                        if (value is null)
                        {
                            options.DefaultSearchOptions.SearchConfiguration.Direction = defaults.DefaultSearchOptions.SearchConfiguration.Direction;
                        }
                        else
                        {
                            if ((error = SearchParseSevice.EnumArgument<SearchDirection>(value, out var res)) is null)
                            {
                                options.DefaultSearchOptions.SearchConfiguration.Direction = res;
                            }
                        }

                        outVal = options.DefaultSearchOptions.SearchConfiguration.Direction.GetString();
                        break;
                    case "searchby":
                        if (value is null)
                        {
                            options.DefaultSearchOptions.SearchConfiguration.SearchFicsBy = defaults.DefaultSearchOptions.SearchConfiguration.SearchFicsBy;
                        }
                        else
                        {
                            if ((error = SearchParseSevice.EnumArgument<SearchBy>(value, out var res)) is null)
                            {
                                options.DefaultSearchOptions.SearchConfiguration.SearchFicsBy = res;
                            }
                        }

                        outVal = options.DefaultSearchOptions.SearchConfiguration.SearchFicsBy.GetString();
                        break;
                    case "rating":
                        if (value is null)
                        {
                            options.DefaultSearchOptions.SearchConfiguration.FicRating = defaults.DefaultSearchOptions.SearchConfiguration.FicRating;
                        }
                        else
                        {
                            if ((error = SearchParseSevice.EnumArgument<Rating>(value, out var res)) is null)
                            {
                                options.DefaultSearchOptions.SearchConfiguration.FicRating = res;
                            }
                        }

                        outVal = options.DefaultSearchOptions.SearchConfiguration.FicRating.GetString();
                        break;
                    case "complete":
                        if (value is null)
                        {
                            options.DefaultSearchOptions.SearchConfiguration.Status = defaults.DefaultSearchOptions.SearchConfiguration.Status;
                        }
                        else
                        {
                            if ((error = SearchParseSevice.EnumArgument<FicStatus>(value, out var res)) is null)
                            {
                                options.DefaultSearchOptions.SearchConfiguration.Status = res;
                            }
                        }

                        outVal = options.DefaultSearchOptions.SearchConfiguration.Status.GetString();
                        break;
                    case "crossover":
                        if (value is null)
                        {
                            options.DefaultSearchOptions.SearchConfiguration.Crossover = defaults.DefaultSearchOptions.SearchConfiguration.Crossover;
                        }
                        else
                        {
                            if ((error = SearchParseSevice.EnumArgument<CrossoverStatus>(value, out var res)) is null)
                            {
                                options.DefaultSearchOptions.SearchConfiguration.Crossover = res;
                            }
                        }

                        outVal = options.DefaultSearchOptions.SearchConfiguration.Crossover.GetString();
                        break;
                    case "cache":
                        if (value is null)
                        {
                            options.CacheFanfics = defaults.CacheFanfics;
                        }
                        else
                        {
                            if ((error = SearchParseSevice.BooleanArgument(value, out var res)) is null)
                            {
                                options.CacheFanfics = res;
                            }
                        }

                        outVal = options.CacheFanfics.ToString();
                        break;
                    case "link":
                        if (value is null)
                        {
                            options.DisplayLinkData = defaults.DisplayLinkData;
                        }
                        else
                        {
                            if ((error = SearchParseSevice.BooleanArgument(value, out var res)) is null)
                            {
                                options.DisplayLinkData = res;
                            }
                        }

                        outVal = options.DisplayLinkData.ToString();
                        break;
                    default:
                        error = new SearchParseResult
                        {
                            Errored = true,
                            ErrorMessage = "Failed to match provided option with a valid config option."
                        };
                        break;
                }

                if (error is null)
                {
                    if (gcfg is not null)
                        db.Update(gcfg);
                    if (user is not null)
                        db.Update(user);
                    await db.SaveChangesAsync();
                    await SendUpdateMessage(ctx, option, outVal, value is null, guild);
                }
                else
                {
                    await RespondError(error.ErrorMessage ?? "An unkown error occoured.");
                }
            }

            private async Task SendUpdateMessage(CommandContext ctx, string option, string value, bool isDefault, bool guild)
            {
                var builder = new DiscordEmbedBuilder().WithColor(Color_Search);
                string guildString = guild ? "the server config" : "your user config";
                if(isDefault)
                {
                    builder.WithDescription($"Set {option} to the default value of {value} for {guildString}.");
                }
                else
                {
                    builder.WithDescription($"Updated {option} to {value} for {guildString}");
                }

                await ctx.RespondAsync(builder);
            }
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
            result -= 1;
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
                            if (user.Options.CacheFanfics)
                            {
                                _ = user.Cache.AddToCahce(fics[result]);

                                db.Update(user);
                                await db.SaveChangesAsync();
                            }

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

            var search = _services.GetRequiredService<SearchParseSevice>();

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

            res.SearchOptions = guild.MultisearchConfiguration.DefaultSearchOptions.Combine(res.SearchOptions ?? new());

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
                $"Search Dir    :: The order wich to order web results. Either ascending (1) or descending (0)." +
                "\n```")
                .AddField("`-s | --searchby <search by>`", "```http\n" +
                $"Usage         :: -s bestmatch\n" +
                $"Usage         :: --searchby 1\n" +
                $"Search By     :: How organize the web results. Must be one of the following: bestmatch (0), likes (1), views (2), updateddate (3)" +
                $" publisheddate (4), or comments (5)." +
                "\n```")
                .AddField("`-R | --rating <rating>`", "```http\n" +
                $"Usage         :: -R any\n" +
                $"Usage         :: --rating 5\n" +
                $"Rating       :: Filter web results by a rating. Must be one of the following: any (0), general (1), teen (2), mature (3)," +
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
                .AddSpecialTypes();
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

    public static class DiscordEmbedBuilderExtensions
    {
        public static DiscordEmbedBuilder AddSpecialTypes(this DiscordEmbedBuilder builder)
        {
            builder.AddField("SPECIAL TYPES",
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
                    "Furthermore, you can specify the amount of weight the numbers have:" +
                    "```http\n" +
                    ">5 days   :: The value 5 is in days.\n" +
                    ">5 weeks  :: The value of 5 is in days times 7 (weeks).\n" +
                    ">5 months :: The value of 5 is in days times 30 (months).\n" +
                    ">5 years  :: The value of 5 is in days times 365 (years).\n" +
                    ">5        :: The value of 5 is in days." +
                    "\n```");

            return builder;
        }
    }
}
