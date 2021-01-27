using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

using CloudNine.Core.Configuration;
using CloudNine.Core.Database;
using CloudNine.Core.Http;
using CloudNine.Core.Multisearch;
using CloudNine.Core.Multisearch.Configuration;
using CloudNine.Core.Multisearch.Requests;

using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace CloudNine.Discord.Interactions
{
    public class FanfictionLinkResponderService : HtmlGrabber
    {
        private readonly IServiceProvider _services;
        private readonly ILogger _logger;

        private readonly IReadOnlyDictionary<LinkType, string> _urls = new Dictionary<LinkType, string>()
        { { LinkType.ao3, "archiveofourown.org" }, /*{ LinkType.ff, "fanfiction.net" },*/ { LinkType.wattpad, "wattpad.com" } };

        public enum LinkType
        {
            ao3,
            ff,
            wattpad
        }

        public FanfictionLinkResponderService(IServiceProvider services, ILogger<FanfictionLinkResponderService> logger, FanfictionClient browser) : base(browser)
        {
            _services = services;
            _logger = logger;
        }


        public Task Client_MessageCreated(DiscordClient sender, MessageCreateEventArgs e)
        {
            _ = Task.Run(async () =>
            {
                try
                {
                    await HandleMessageCreatedAsync(sender, e);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Fanfiction Link Service errored");
                }
            });

            return Task.CompletedTask;
        }

        private async Task HandleMessageCreatedAsync(DiscordClient sender, MessageCreateEventArgs e)
        {
            bool potentialLink = false;
            foreach (var b in _urls)
            {
                if (e.Message.Content.Contains(b.Value))
                    potentialLink = true;

                if (potentialLink) break;
            }

            if (potentialLink)
            {
                var db = _services.GetRequiredService<CloudNineDatabaseModel>();
                var guild = await db.FindAsync<DiscordGuildConfiguration>(e.Guild.Id);
                if (guild is null || guild.MultisearchConfiguration.DisplayLinkData)
                {
                    var user = await db.FindAsync<MultisearchUser>(e.Message.Author.Id);
                    if (user is null || user.Options.DisplayLinkData)
                    {
                        var links = ParseValidLinks(e.Message.Content);

                        if (links.Count != 0)
                        {
                            SearchOptions ops = guild?.MultisearchConfiguration.DefaultSearchOptions.Combine(user?.Options.DefaultSearchOptions ?? new()) ?? new();

                            var res = await HandleWebRequests(links, ops);

                            if (res.Count != 0)
                            {
                                await RespondToUser(res, e.Channel, guild?.MultisearchConfiguration ?? new(), user?.Options ?? new());

                                if (guild is not null && guild.MultisearchConfiguration.CacheFanfics)
                                {
                                    foreach (var f in res)
                                        _ = guild.MultisearchCache.AddToCahce(f);

                                    db.Update(guild);
                                    await db.SaveChangesAsync();
                                }

                                if (user is not null && user.Options.CacheFanfics)
                                {
                                    foreach (var f in res)
                                        _ = user.Cache.AddToCahce(f);

                                    db.Update(user);
                                    await db.SaveChangesAsync();
                                }
                            }
                        }
                    }
                }
            }
        }

        private List<string> ParseValidLinks(string data)
        {
            var links = new List<string>();

            var blank = string.Join(" ", data.Split("\n", StringSplitOptions.RemoveEmptyEntries));
            var raw = blank.Split(" ", StringSplitOptions.RemoveEmptyEntries);

            bool ignore = false;
            foreach(var entry in raw)
            {
                if (entry.Contains("```"))
                    ignore = !ignore;

                if(!ignore)
                {
                    if (Uri.TryCreate(entry, UriKind.Absolute, out var uri))
                        links.Add(uri.AbsoluteUri);
                }
            }

            return links;
        }

        private async Task<List<FanFic>> HandleWebRequests(List<string> urls, SearchOptions searchOptions)
        {
            List<RequestBase> requests = new();

            foreach (var link in urls)
            {
                foreach(var type in _urls)
                {
                    if(link.Contains(type.Value))
                    {
                        bool added = false;
                        switch(type.Key)
                        {
                            case LinkType.ao3:
                                requests.Add(await GetAO3FicRequest(link));
                                added = true;
                                break;
                            case LinkType.ff:
                                requests.Add(GetFanfictionFicRequest(link));
                                added = true;
                                break;
                            case LinkType.wattpad:
                                requests.Add(await GetWattpadFicRequest(link));
                                added = true;
                                break;
                        }

                        if (added) break;
                    }
                }
            }

            var manager = new FanfictionLinkManager(requests, Web, searchOptions);

            var res = await manager.MakeWebRequest();

            return res;
        }

        private async Task<AO3FicRequest> GetAO3FicRequest(string url)
        {
            string ficid = "";
            if (url.Contains("works/"))
            {
                ficid = url[(url.IndexOf("works/") + 6)..];

                var endOfWork = ficid.IndexOf("/");

                if(endOfWork > 0)
                {
                    ficid = ficid.Substring(0, endOfWork);
                }
            }
            else
            {
                string cid = url[(url.IndexOf("chapters/") + 9)..];

                var endofChapter = cid.IndexOf("/");

                if (endofChapter > 0)
                {
                    cid = cid.Substring(0, endofChapter);
                }

                var cr = new AO3ChapterRequest(cid);

                var html = await GetHtmlAsync(cr.GetRequestString());

                cr.Result.LoadHtml(html);
                cr.FixBasicErrors();

                ficid = cr.GetWorkId();
            }

            return new(ficid, url);
        }

        private FanfictionFicRequest GetFanfictionFicRequest(string url)
            => new(url);

        private async Task<WattpadFicRequest> GetWattpadFicRequest(string url)
        {
            string wid = "";
            if (url.Contains("story/"))
            {
                wid = url[(url.IndexOf("story/") + 6)..];

                var endOfWork = wid.IndexOf("/");

                if (endOfWork > 0)
                {
                    wid = wid.Substring(0, endOfWork);
                }
            }
            else
            {
                string cid = url[(url.IndexOf("com/") + 4)..];

                var endofChapter = cid.IndexOf("-");

                if (endofChapter > 0)
                {
                    cid = cid.Substring(0, endofChapter);
                }

                var cr = new WattpadChapterRequest(cid);

                var html = await GetHtmlAsync(cr.GetRequestString());

                cr.Result.LoadHtml(html);
                cr.FixBasicErrors();

                wid = cr.GetWorkId();
            }

            return new(wid, url);
        }

        private async Task RespondToUser(List<FanFic> fics, DiscordChannel discordChannel, 
            MultisearchConfigurationOptions guildOptions, MultisearchConfigurationOptions userOptions)
        {
            foreach (var f in fics)
                foreach (var part in f.GetDiscordEmbeds(guildOptions, userOptions) ?? Enumerable.Empty<DiscordEmbedBuilder>())
                    await discordChannel.SendMessageAsync(part);
        }
    }
}
