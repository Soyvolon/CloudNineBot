using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using CloudNine.Core.Configuration;
using CloudNine.Core.Database;
using CloudNine.Core.Multisearch;
using CloudNine.Core.Multisearch.Configuration;

using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity.Enums;
using DSharpPlus.Interactivity.Extensions;

using Microsoft.Extensions.DependencyInjection;

namespace CloudNine.Discord.Commands.Multiserach
{
    public partial class MultisearchModule
    {
        [Group("cache")]
        [Description("Commands related to the search cache.")]
        public class CacheCommands : CommandModule
        {
            private readonly IServiceProvider _services;

            private readonly List<string> positiveResponses = new List<string>() { "yes", "y" };

            public CacheCommands(IServiceProvider services)
            {
                _services = services;
            }

            [GroupCommand]
            [Description("Show the cache for your account.")]
            public async Task ViewCacheAsync(CommandContext ctx)
            {
                var db = _services.GetRequiredService<CloudNineDatabaseModel>();
                var user = await db.FindAsync<MultisearchUser>(ctx.User.Id);

                await DisplayCahceItems(ctx, user.Cache.Cache, user.Cache.History);
            }

            [Command("guild")]
            [Aliases("server")]
            [Description("Show the cache for this server.")]
            public async Task ViewServerCacheAsync(CommandContext ctx)
            {
                var db = _services.GetRequiredService<CloudNineDatabaseModel>();
                var guild = await db.FindAsync<DiscordGuildConfiguration>(ctx.Guild.Id);

                await DisplayCahceItems(ctx, guild.MultisearchCache.Cache, guild.MultisearchCache.History);
            }

            private async Task DisplayCahceItems(CommandContext ctx, List<FanFic> fics, List<Tuple<string, string>> history)
            {
                List<string> data = new();
                int i = 1;
                foreach (var fic in fics)
                    data.Add($"**{i++}.** {fic.Title.Item1} by {fic.Author.Item1}" +
                    $" **--** {(fic.Description.Length > 100 ? $"{fic.Description[..97].Trim()}..." : fic.Description.Trim())}");

                foreach (var d in history)
                    data.Add($"**{i++}** [{d.Item1}]({d.Item2})");

                if(data.Count <= 0)
                {
                    await RespondWarn("No cache items found.");
                    return;
                }

                var content = string.Join("\n\n", data);

                var interact = ctx.Client.GetInteractivity();

                var pages = interact.GeneratePagesInEmbed(content, SplitType.Line, InteractBase());

                _ = interact.SendPaginatedMessageAsync(ctx.Channel, ctx.User, pages);
            }

            [Command("details")]
            [Description("Show the details of a cahce item.")]
            public async Task CacheDetailsCommandAsync(CommandContext ctx, int item = 1)
            {
                item--;

                if(item < 0)
                {
                    await RespondError("Item value must be greater than 0.");
                    return;
                }

                var db = _services.GetRequiredService<CloudNineDatabaseModel>();
                var guild = await db.FindAsync<DiscordGuildConfiguration>(ctx.Guild.Id);
                var user = await db.FindAsync<MultisearchUser>(ctx.User.Id);

                if(user is null)
                {
                    await RespondError("No cache items exsist.");
                    return;
                }

                if (user.Cache.TryGetValue(item, out var res))
                {
                    await ShowDetailsAsync(ctx, res, guild?.MultisearchConfiguration ?? new(), user?.Options ?? new());
                }
                else
                {
                    await RespondError($"Failed to get an item from the cache with the ID of {item}");
                }
            }

            [Command("guildetails")]
            [Aliases("serverdetails", "gdetails", "sdetails")]
            [Description("Show the details of a server's cache item.")]
            public async Task GuildCacheDetailsCommandAsync(CommandContext ctx, int item = 1)
            {
                item--;

                if(item < 0)
                {
                    await RespondError("Item value must be greater than 0.");
                    return;
                }

                var db = _services.GetRequiredService<CloudNineDatabaseModel>();
                var guild = await db.FindAsync<DiscordGuildConfiguration>(ctx.Guild.Id);
                var user = await db.FindAsync<MultisearchUser>(ctx.User.Id);

                if(guild is null)
                {
                    await RespondError("No cache items exsist.");
                    return;
                }

                if (guild.MultisearchCache.TryGetValue(item, out var res))
                {
                    await ShowDetailsAsync(ctx, res, guild?.MultisearchConfiguration ?? new(), user?.Options ?? new());
                }
                else
                {
                    await RespondError($"Failed to get an item from the cache with the ID of {item}");
                }
            }

            private async Task ShowDetailsAsync(CommandContext ctx, object res,
                MultisearchConfigurationOptions guildOptions, MultisearchConfigurationOptions userOptions)
            {
                if (res is FanFic fic)
                    await ShowDetailsAsync(ctx, fic, guildOptions, userOptions);
                else if (res is Tuple<string, string> hist)
                    await ShowDetailsAsync(ctx, hist);
                else await RespondError("Failed to parse result to proper item type.");
            }

            private async Task ShowDetailsAsync(CommandContext ctx, FanFic item,
                MultisearchConfigurationOptions guildOptions, MultisearchConfigurationOptions userOptions)
                => await ShowDetailsAsync(ctx, builders: item.GetDiscordEmbeds(guildOptions, userOptions));

            private async Task ShowDetailsAsync(CommandContext ctx, Tuple<string, string> item)
            {
                var builder = new DiscordEmbedBuilder();
                builder.WithTitle(item.Item1).WithUrl(item.Item2);
                await ShowDetailsAsync(ctx, builders: new() { builder });
            }

            private async Task ShowDetailsAsync(CommandContext ctx, List<DiscordEmbedBuilder>? builders)
            {
                foreach (var builder in builders ?? Enumerable.Empty<DiscordEmbedBuilder>())
                    await ctx.RespondAsync(builder);
            }

            [Command("userclear")]
            [Description("Clears the cache for a user, or a single item if specified.")]
            public async Task ClearUserCacheAsync(CommandContext ctx, int item = 0)
            {
                item--;

                if(item < -1)
                {
                    await RespondError("Item can not be less than 0");
                    return;
                }

                var db = _services.GetRequiredService<CloudNineDatabaseModel>();
                var user = await db.FindAsync<MultisearchUser>(ctx.User.Id);

                if(user is null)
                {
                    await RespondError("No cache to clear.");
                    return;
                }

                var interact = ctx.Client.GetInteractivity();

                if (item == -1)
                {
                    await RespondWarn("You are about to purge the multisearch cache for your account." +
                        " All cached fanfics and fanfic history will be removed." +
                        " Are you sure you wish to continue? `(Y)es/(N)o`");

                    var confirm = await interact.WaitForMessageAsync(x => x.Author.Id == ctx.User.Id);

                    if (confirm.TimedOut)
                        await RespondError("Request timed out.");
                    else if (!positiveResponses.Contains(confirm.Result.Content.ToLower()))
                        await RespondError("Canceling purge.");
                    else
                    {
                        user.Cache = new();
                        db.Update(user);
                        await db.SaveChangesAsync();

                        await RespondWarn("Cache Cleared.");
                    }
                }
                else
                {
                    if (user.Cache.TryGetValue(item, out var res))
                    {
                        string? title = res switch
                        {
                            FanFic fic => fic.Title.Item1,
                            Tuple<string, string> pair => pair.Item1,
                            _ => null
                        };

                        if (title is null)
                        {
                            await RespondError("No proper value converted to be deleted.");
                        }
                        else
                        {
                            await RespondWarn($"You are about to purge {title} from your account." +
                                " This fanfic will be removed." +
                                " Are you sure you wish to continue? `(Y)es/(N)o`");

                            var confirm = await interact.WaitForMessageAsync(x => x.Author.Id == ctx.User.Id);

                            if (confirm.TimedOut)
                                await RespondError("Request timed out.");
                            else if (!positiveResponses.Contains(confirm.Result.Content.ToLower()))
                                await RespondError("Canceling purge.");
                            else
                            {
                                if (res is FanFic fic) user.Cache.Cache.Remove(fic);
                                else if (res is Tuple<string, string> tup) user.Cache.History.Remove(tup);

                                db.Update(user);
                                await db.SaveChangesAsync();

                                await RespondWarn($"Removed {title} from cache.");
                            }
                        }
                    }
                    else
                    {
                        await RespondError($"Failed to get an item from the cache with the ID of {item}");
                    }
                }
            }

            [Command("guildclear")]
            [Aliases("serverclear")]
            [Priority(4)]
            [Description("Clears the cahce for the server, or a single item if specified.")]
            [RequireUserPermissions(DSharpPlus.Permissions.ManageMessages)]
            public async Task ClearGuildCacheAsync(CommandContext ctx, int item = 0)
            {
                item--;

                if (item < -1)
                {
                    await RespondError("Item can not be less than 0");
                    return;
                }

                var db = _services.GetRequiredService<CloudNineDatabaseModel>();
                var guild = await db.FindAsync<DiscordGuildConfiguration>(ctx.Guild.Id);

                if(guild is null)
                {
                    await RespondError("No cache to clear.");
                    return;
                }

                var interact = ctx.Client.GetInteractivity();

                if (item == -1)
                {
                    await RespondWarn("You are about to purge the multisearch cache for your server." +
                        " All cached fanfics and fanfic history will be removed." +
                        " Are you sure you wish to continue? `(Y)es/(N)o`");

                    var confirm = await interact.WaitForMessageAsync(x => x.Author.Id == ctx.User.Id);

                    if (confirm.TimedOut)
                        await RespondError("Request timed out.");
                    else if (!positiveResponses.Contains(confirm.Result.Content.ToLower()))
                        await RespondError("Canceling purge.");
                    else
                    {
                        guild.MultisearchCache = new();
                        db.Update(guild);
                        await db.SaveChangesAsync();

                        await RespondWarn("Cache Cleared.");
                    }
                }
                else
                {
                    if (guild.MultisearchCache.TryGetValue(item, out var res))
                    {
                        string? title = res switch
                        {
                            FanFic fic => fic.Title.Item1,
                            Tuple<string, string> pair => pair.Item1,
                            _ => null
                        };

                        if(title is null)
                        {
                            await RespondError("No proper value converted to be deleted.");
                        }
                        else
                        {
                            await RespondWarn($"You are about to purge {title} from your server." +
                                " This fanfic will be removed." +
                                " Are you sure you wish to continue? `(Y)es/(N)o`");

                            var confirm = await interact.WaitForMessageAsync(x => x.Author.Id == ctx.User.Id);

                            if (confirm.TimedOut)
                                await RespondError("Request timed out.");
                            else if (!positiveResponses.Contains(confirm.Result.Content.ToLower()))
                                await RespondError("Canceling purge.");
                            else
                            {
                                if (res is FanFic fic) guild.MultisearchCache.Cache.Remove(fic);
                                else if (res is Tuple<string, string> tup) guild.MultisearchCache.History.Remove(tup);

                                db.Update(guild);
                                await db.SaveChangesAsync();

                                await RespondWarn($"Removed {title} from cache.");
                            }
                        }
                    }
                    else
                    {
                        await RespondError($"Failed to get an item from the cache with the ID of {item}");
                    }
                }
            }
        }
    }
}
