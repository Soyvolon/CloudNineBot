using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using CloudNine.Core.Multisearch;

using DSharpPlus.Entities;

namespace CloudNine.Discord.Interactions
{
    public class MultisearchInteraction
    {

        public const int split = 10;

        public DiscordMessage? Message { get; set; }
        public int DisplayPage { get; set; }

        public SearchManager Manager { get; init; }
        public DiscordChannel Channel { get; init; }
        public DiscordUser User { get; init; }

        public List<FanFic> DisplayCache { get; init; }
        public List<string> DataCache { get; init; }
        private Timer TimeoutTimer { get; init; }

        private bool Stopped { get; set; }
        private bool PendingLoad { get; set; }

        public MultisearchInteraction(SearchManager manager, DiscordChannel channel, DiscordUser user)
        {
            Manager = manager;
            Channel = channel;
            User = user;
            DisplayPage = 1;

            DisplayCache = new();
            DataCache = new();
            Stopped = true;
            PendingLoad = false;
            TimeoutTimer = new(async (x) => await StopAsync(), null, Timeout.InfiniteTimeSpan, Timeout.InfiniteTimeSpan);
        }

        public async Task<DiscordMessage> StartAsync(DiscordEmbedBuilder baseEmbed)
        {
            if(Manager.TryGetCurrentPageResults(out var fics))
            {
                DisplayCache.AddRange(fics);
                DataCache.AddRange(GenerateDetails(fics));
            }

            baseEmbed.WithFooter($"Page: {DisplayPage} | Loaded Web Pages: {Manager.WebsitePageNumber}")
                .WithDescription(GetPage(DataCache, DisplayPage, out _));

            var msg = await Channel.SendMessageAsync(baseEmbed);

            Message = msg;

            Stopped = false;
            TimeoutTimer.Change(TimeSpan.FromMinutes(2), Timeout.InfiniteTimeSpan);

            return msg;
        }

        public async Task NextPageAsync()
        {
            if (Message is not null)
            {
                TimeoutTimer.Change(TimeSpan.FromMinutes(2), Timeout.InfiniteTimeSpan);

                if (PendingLoad)
                {
                    await LoadNextDataSetAsync();
                    return;
                }
                
                var e = Message.Embeds[0];
                var builder = new DiscordEmbedBuilder(e);

                var page = GetPage(DataCache, ++DisplayPage, out int last);
                
                builder.WithDescription(page)
                    .WithFooter($"Page: {DisplayPage} | Loaded Web Pages: {Manager.WebsitePageNumber}");

                if(last >= DataCache.Count)
                {
                    builder.AddField("Load more results:", "Press the next button to load more results.");
                    PendingLoad = true;
                }

                await Message.ModifyAsync(embed: builder.Build());
            }
        }

        public async Task PreviousPageAsync()
        {
            if(Message is not null && DisplayPage > 1)
            {
                TimeoutTimer.Change(TimeSpan.FromMinutes(2), Timeout.InfiniteTimeSpan);

                var e = Message.Embeds[0];
                var builder = new DiscordEmbedBuilder(e);

                var page = GetPage(DataCache, --DisplayPage, out _);

                builder.WithDescription(page)
                    .WithFooter($"Page: {DisplayPage} | Loaded Web Pages: {Manager.WebsitePageNumber}");

                PendingLoad = false; // cant pend a load if we are not on the last page.

                await Message.ModifyAsync(embed: builder.Build());
            }
        }

        public async Task StopAsync()
        {
            if (!Stopped)
            {
                await Message!.DeleteAllReactionsAsync();
                Stopped = true;
            }
        }

        private async Task LoadNextDataSetAsync()
        {
            if(Message is not null)
            {
                var e = Message.Embeds[0];
                var builder = new DiscordEmbedBuilder(e);
                builder.WithColor(DiscordColor.Yellow)
                    .WithDescription("Loading more results...")
                    .WithFooter($"Page: {DisplayPage} | Loaded Web Pages: {Manager.WebsitePageNumber}");

                await Message.ModifyAsync(embed: builder.Build());

                await Manager.LoadNextSearchPage();

                if(Manager.TryGetCurrentPageResults(out var res))
                {
                    var newDetails = GenerateDetails(res, DisplayCache.Count + 1);
                    DisplayCache.AddRange(res);
                    DataCache.Add($"\n**-- Page {Manager.WebsitePageNumber} --**\n");
                    DataCache.AddRange(newDetails);

                    PendingLoad = false;
                    DisplayPage--; // drop this back one to load the current page in the next page funcion.
                    await NextPageAsync();
                }
                else
                {
                    builder.WithColor(DiscordColor.DarkRed)
                        .WithDescription("Failed to load more data, closing...");

                    await Message.ModifyAsync(embed: builder.Build());

                    await StopAsync();
                }
            }

            PendingLoad = false;
        }

        public static string GetPage(List<string> data, int page, out int lastPos)
        {
            var start = (page - 1) * split;
            lastPos = start + split;
            var splitData = data.GetRange(start, data.Count > lastPos ? split : data.Count - start);
            return string.Join("\n\n", splitData);
        }

        public static List<string> GenerateDetails(List<FanFic> fics, int start = 1)
        {
            int c = start;
            List<string> data = new();
            foreach (var fic in fics)
                data.Add($"**{c++}.** {fic.Title.Item1} by {fic.Author.Item1}" +
                    $" **--** {(fic.Description.Length > 100 ? $"{fic.Description[..97].Trim()}..." : fic.Description.Trim())}");

            return data;
        }
    }
}
