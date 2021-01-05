using System;
using System.Collections.Generic;
using System.Linq;

using CloudNine.Core.Multisearch.Configuration;
using CloudNine.Core.Multiserach;

using DSharpPlus.Entities;

using Newtonsoft.Json;

namespace CloudNine.Core.Multisearch
{
    public class FanFic
    {
        [JsonProperty("title")]
        public Tuple<string, string> Title { get; set; }
        [JsonProperty("author")]
        public Tuple<string, string> Author { get; set; }
        [JsonProperty("fandoms")]
        public List<Tuple<string, string>> Fandoms { get; set; }
        [JsonProperty("tags")]
        public List<Tuple<string, string>> Tags { get; set; }
        [JsonProperty("character_tags")]
        public List<Tuple<string, string>> CharacterTags { get; set; }
        [JsonProperty("relationship_tags")]
        public List<Tuple<string, string>> RelationshipTags { get; set; }
        [JsonProperty("likes")]
        public long Likes { get; set; }
        [JsonProperty("views")]
        public long Views { get; set; }
        [JsonProperty("description")]
        public string Description { get; set; }
        [JsonProperty("paid_story")]
        public bool? PaidStory { get; set; }
        [JsonProperty("completed")]
        public bool Completed { get; set; }
        [JsonProperty("site")]
        public SiteFrom Site { get; set; }

        public FanFic()
        {
            Tags = new();
            Fandoms = new();
            Likes = 0;
            PaidStory = false;
            Completed = false;
            Title = new(string.Empty, string.Empty);
            Author = new(string.Empty, string.Empty);
            Description = string.Empty;
        }

        /// <summary>
        /// Adds a tag to the FanFic's Tag list
        /// </summary>
        /// <param name="tag">Tag to add</param>
        public void AddTag(Tuple<string,string> tag)
        {
            Tags.Add(tag);
        }

        /// <summary>
        /// Removes a tag from the FanFic's tag list
        /// </summary>
        /// <param name="tag">Tag to remove</param>
        /// <returns>True if tag was removed</returns>
        public bool RemoveTag(Tuple<string, string> tag)
        {
            return Tags.Remove(tag);
        }

        public List<DiscordEmbedBuilder> GetDiscordEmbeds(MultisearchConfigurationOptions guildOptions, MultisearchConfigurationOptions userOptions)
        {
            var options = new MultisearchConfigurationOptions()
            {
                CharacterTagLimit = Math.Min(guildOptions.CharacterTagLimit, userOptions.CharacterTagLimit),
                DefaultSearchOptions = new()
                {
                    ItemsPerPage = Math.Min(guildOptions.DefaultSearchOptions.ItemsPerPage, userOptions.DefaultSearchOptions.ItemsPerPage)
                },
                HideSensitiveContentDescriptions = guildOptions.HideSensitiveContentDescriptions || userOptions.HideSensitiveContentDescriptions,
                OverflowDescription = guildOptions.OverflowDescription || userOptions.OverflowDescription,
                RelationshipTagLimit = Math.Min(guildOptions.CharacterTagLimit, userOptions.CharacterTagLimit),
                TagLimit = Math.Min(guildOptions.TagLimit, userOptions.TagLimit)
            };

            return GetDiscordEmbeds(options);
        }

        public List<DiscordEmbedBuilder> GetDiscordEmbeds(MultisearchConfigurationOptions options)
        {
            var res = new List<DiscordEmbedBuilder>();

            var primary = new DiscordEmbedBuilder();
            primary.WithAuthor(Author.Item1, Author.Item2)
                .WithTitle(Title.Item1)
                .WithUrl(Title.Item2);

            res.Add(primary);

            // drop content size by four in order to fit spoiler tags if needed.
            if (Description.Length > 2044 && options.OverflowDescription)
            {
                primary.WithDescription(Description[..2043]);
                var remainderString = Description[2044..];

                int remaining = remainderString.Length;
                while(remaining > 0)
                {
                    var embed = new DiscordEmbedBuilder();
                    if(remaining > 2044)
                    {
                        embed.WithDescription(remainderString[..2043]);
                        remainderString = remainderString[2044..];
                        remaining = remainderString.Length;
                    }
                    else
                    {
                        embed.WithDescription(remainderString);
                        remaining = 0;
                    }
                    res.Add(embed);
                }
            }
            else
            {
                primary.WithDescription(Description);
            }

            AddTags(options.RelationshipTagLimit, RelationshipTags, "Relationships", ref res);
            AddTags(options.CharacterTagLimit, CharacterTags, "Characters", ref res);
            AddTags(options.TagLimit, Tags, "Other Tags", ref res);

            if (options.HideSensitiveContentDescriptions)
            {
                // do search for archive warnings, ff.net warnings, etc.
                foreach(var e in res)
                {
                    e.Description = e.Description.Insert(0, "||");
                    e.Description = e.Description.Insert(e.Description.Length - 1, "||");
                }
            }

            foreach(var e in res)
            {
                switch (Site)
                {
                    case SiteFrom.ArchiveOfOurOwn:
                        e.WithFooter("Archive of Our Own", "https://archiveofourown.org/images/ao3_logos/logo_42.png");
                        break;
                    case SiteFrom.FanfictionDotNet:
                        e.WithFooter("Fanfiction.net", "https://pbs.twimg.com/profile_images/843841615122784256/WXbuqyjo_400x400.jpg");
                        break;
                    case SiteFrom.Wattpad:
                        e.WithFooter("Wattpad", "https://www.wattpad.com/brand/placeholder/w_placeholder-b82c454c177d04c66a13c13cff05b539.png");
                        break;
                }
            }

            return res;
        }

        private static void AddTags(int limit, List<Tuple<string, string>> tags, string title, ref List<DiscordEmbedBuilder> result)
        {
            if (tags is null)
                return;

            string data;
            if (limit != 0 && tags.Count > limit)
            {
                if (limit != -1)
                {
                    var tList = new List<string>();

                    for (int i = 0; i < tags.Count || i < limit; i++)
                    {
                        tList.Add(tags[i].Item1);
                    }

                    data = string.Join(", ", tList);
                }
                else return;
            }
            else
            {
                var tList = new List<string>();

                foreach (var t in tags)
                    tList.Add(t.Item1);

                data = string.Join(", ", tList);
            }

            var embed = result.Last();

            if (data.Length > 1024)
            {
                int c = 0;
                while (data.Length > 1024)
                {
                    embed.AddField($"Tags {(c == 0 ? "" : $"{c}")}", data[..1023]);
                    data = data[1024..];
                }
            }
            else
            {
                embed.AddField("Tags", data);
            }
        }
    }
}