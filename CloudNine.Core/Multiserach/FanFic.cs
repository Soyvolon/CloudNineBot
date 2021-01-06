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
        public List<Tuple<string, string>> Fandoms { get; set; } = new();
        [JsonProperty("tags")]
        public List<Tuple<string, string>> Tags { get; set; } = new();
        [JsonProperty("character_tags")]
        public List<Tuple<string, string>> CharacterTags { get; set; } = new();
        [JsonProperty("relationship_tags")]
        public List<Tuple<string, string>> RelationshipTags { get; set; } = new();
        [JsonProperty("likes")]
        public long Likes { get; set; } = 0;
        [JsonProperty("views")]
        public long Views { get; set; } = 0;
        [JsonProperty("description")]
        public string Description { get; set; } = "";
        [JsonProperty("paid_story")]
        public bool? PaidStory { get; set; } = null;
        [JsonProperty("completed")]
        public bool Completed { get; set; } = false;
        [JsonProperty("rating")]
        public string Rating { get; set; } = "";
        [JsonProperty("sensitive_content_warning")]
        public bool SensitiveContentWarning { get; set; } = false;
        [JsonProperty("explicit")]
        public bool IsExplicit { get; set; } = false;
        [JsonProperty("site")]
        public SiteFrom Site { get; set; }

        public FanFic()
        {

        }

        /// <summary>
        /// Adds a tag to the FanFic's Tag list
        /// </summary>
        /// <param name="tag">Tag to add</param>
        public void AddTag(Tuple<string,string> tag)
            => Tags.Add(tag);
        
        /// <summary>
        /// Adds a tag to the Fanfic's character tag list
        /// </summary>
        /// <param name="tag">Tag to add</param>
        public void AddCharacterTag(Tuple<string, string> tag)
            => CharacterTags.Add(tag);

        /// <summary>
        /// Adds a tag to the Fanfic's relationship tag list
        /// </summary>
        /// <param name="tag">Tag to add</param>
        public void AddRelationshipTag(Tuple<string, string> tag)
            => RelationshipTags.Add(tag);

        /// <summary>
        /// Removes a tag from the FanFic's tag list
        /// </summary>
        /// <param name="tag">Tag to remove</param>
        /// <returns>True if tag was removed</returns>
        public bool RemoveTag(Tuple<string, string> tag)
        {
            return Tags.Remove(tag);
        }

        public List<DiscordEmbedBuilder>? GetDiscordEmbeds(MultisearchConfigurationOptions guildOptions, MultisearchConfigurationOptions userOptions)
        {
            var options = new MultisearchConfigurationOptions()
            {
                CharacterTagLimit = Math.Min(guildOptions.CharacterTagLimit, userOptions.CharacterTagLimit),
                DefaultSearchOptions = new()
                {
                    ItemsPerPage = Math.Min(guildOptions.DefaultSearchOptions.ItemsPerPage, userOptions.DefaultSearchOptions.ItemsPerPage),
                    AllowExplicit = guildOptions.DefaultSearchOptions.AllowExplicit && userOptions.DefaultSearchOptions.AllowExplicit
                },
                HideSensitiveContentDescriptions = guildOptions.HideSensitiveContentDescriptions || userOptions.HideSensitiveContentDescriptions,
                OverflowDescription = guildOptions.OverflowDescription || userOptions.OverflowDescription,
                RelationshipTagLimit = Math.Min(guildOptions.CharacterTagLimit, userOptions.CharacterTagLimit),
                TagLimit = Math.Min(guildOptions.TagLimit, userOptions.TagLimit)
            };

            return GetDiscordEmbeds(options);
        }

        public List<DiscordEmbedBuilder>? GetDiscordEmbeds(MultisearchConfigurationOptions options)
        {
            var res = new List<DiscordEmbedBuilder>();

            var primary = new DiscordEmbedBuilder();
            primary.WithTitle(Title.Item1);

            try
            {
                primary.WithUrl(Title.Item2);
            }
            catch (Exception ex)
            { // cant send if the story URL is invalid.
                return null;
            }

            try
            {
                primary.WithAuthor(Author.Item1, Author.Item2);
            }
            catch
            {
                primary.WithAuthor(Author.Item1);
            }

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

            ApplyDetails(ref res);

            AddTags(0, Fandoms, "Fandoms", ref res);
            AddTags(options.RelationshipTagLimit, RelationshipTags, "Relationships", ref res);
            AddTags(options.CharacterTagLimit, CharacterTags, "Characters", ref res);
            AddTags(options.TagLimit, Tags, "Other Tags", ref res);

            if (SensitiveContentWarning && options.HideSensitiveContentDescriptions)
            {
                // do search for archive warnings, ff.net warnings, etc.
                foreach(var e in res)
                {
                    if (!string.IsNullOrWhiteSpace(e.Description))
                    {
                        e.Description = e.Description.Insert(0, "||");
                        e.Description = e.Description.Insert(e.Description.Length - 1, "||");
                    }
                }
            }

            foreach(var e in res)
            {
                switch (Site)
                {
                    case SiteFrom.ArchiveOfOurOwn:
                        e.WithFooter("Archive of Our Own", "https://archiveofourown.org/images/ao3_logos/logo_42.png")
                            .WithColor(DiscordColor.DarkRed);
                        break;
                    case SiteFrom.FanfictionDotNet:
                        e.WithFooter("Fanfiction.net", "https://pbs.twimg.com/profile_images/843841615122784256/WXbuqyjo_400x400.jpg")
                            .WithColor(DiscordColor.Blue);
                        break;
                    case SiteFrom.Wattpad:
                        e.WithFooter("Wattpad", "https://www.wattpad.com/brand/placeholder/w_placeholder-b82c454c177d04c66a13c13cff05b539.png")
                            .WithColor(DiscordColor.Orange);
                        break;
                }

                if (string.IsNullOrWhiteSpace(e.Description))
                    e.WithDescription("No Description Provided.");
            }

            return res;
        }

        private void ApplyDetails(ref List<DiscordEmbedBuilder> res)
        {
            var last = res.Last();

            last.AddField("Fic Details",
                $"```http\n" +
                $"Likes           :: {Likes:N0}\n" +
                $"Views           :: {Views:N0}\n" +
                $"Completed       :: {Completed}\n" +
                $"```\n" +
                $"```http\n" +
                $"Rating          :: {Rating}\n" +
                $"Content Warning :: {SensitiveContentWarning}\n" +
                $"Explicit        :: {IsExplicit}" +
                $"\n```");
        }

        private static void AddTags(int limit, List<Tuple<string, string>> tags, string title, ref List<DiscordEmbedBuilder> result)
        {
            if (tags is null || tags.Count <= 0)
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
                int c = 1;
                while (data.Length > 1024)
                {
                    var lastComma = data[..1024].LastIndexOf(",");

                    embed.AddField($"{title} {(c == 1 ? "" : $"{c}")}", data.Substring(0, lastComma));
                    data = data.Substring(lastComma + 2);
                    c++;
                }
            }
            else
            {
                embed.AddField($"{title}", data);
            }
        }
    }
}