using CloudNine.Core.Multisearch.Configuration;

using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CloudNine.Core.Multisearch.Requests
{
    public class WattpadRequest : WattpadBase
    {
        private const string request_base = "https://www.wattpad.com/search/";
        private const string tags_base = "%20%23";

        public WattpadRequest() : base()
        {

        }

        public WattpadRequest(Search search) : base(search)
        {
            // Wattpad specefic search querys
            // oh wait there are none because wattpad is trash!
            Characters = search.Characters;
            Fandoms = search.Fandoms;
            OtherTags = search.OtherTags;
        }

        private string CompileTagString()
        {
            string tags = string.Empty;
            if(Characters is not null)
                Characters.ForEach(x => tags += tags_base + x);
            if(Fandoms is not null)
                Fandoms.ForEach(x => tags += tags_base + x); // all three take each item and add it to the tags string with tags_base before it
            if(OtherTags is not null)
                OtherTags.ForEach(x => tags += tags_base + x);
            tags.Trim();
            return tags;
        }

        public override string GetRequestString(int pageNumber = 1)
        {
            return request_base + Query + CompileTagString();
        }

        public override List<FanFic> DecodeHTML(SearchOptions searchOptions)
        {
            var nodes = Result.DocumentNode.SelectNodes("//div[contains(@class, 'results-story-item')]");

            List<FanFic> fics = new List<FanFic>();

            if (nodes != null)
            {
                foreach (HtmlNode node in nodes)
                { // TODO: Determine error problems when reading information
                    try
                    {
                        FanFic fic = new FanFic()
                        {
                            Site = Multisearch.SiteFrom.Wattpad
                        };

                        // Gather basic story info
                        var title_node = node.SelectSingleNode(".//h5[contains(@class, 'story-title-heading')]");
                        var title_link_node = node.SelectSingleNode(".//a[contains(@class, 'on-result')]");
                        var author_node = node.SelectSingleNode(".//div[contains(@class, 'cover')]//img");

                        fic.Title = new Tuple<string, string>(title_node.InnerText, link_base + title_link_node.Attributes["href"].Value);
                        fic.Author = new Tuple<string, string>(author_node.Attributes["alt"].Value.Split(" by ").Last().Trim(), "");

                        // Gather description
                        var desc_node = node.SelectSingleNode(".//div[contains(@class, 'content')]//p");
                        fic.Description = desc_node.InnerText;

                        // Is this story paid?
                        var paid_node = node.SelectSingleNode(".//div[contains(@class, 'paid-indicator')]");
                        if (paid_node != null)
                        {
                            fic.PaidStory = true;
                        }

                        // Get likes and views
                        var meta_node = node.SelectSingleNode(".//div[contains(@class, 'meta')]");

                        try
                        {
                            var reads_node = meta_node.SelectSingleNode(".//small[contains(@class, 'reads')]");
                            if (ConvertStringMetaNumbers(reads_node.InnerText, out ulong views))
                            {
                                fic.Views = views;
                            }
                        }
                        catch { /* non-essential */ }


                        try
                        {
                            var likes_node = meta_node.SelectSingleNode(".//small[contains(@class, 'votes')]");
                            if (ConvertStringMetaNumbers(likes_node.InnerText, out ulong likes))
                            {
                                fic.Likes = likes;
                            }
                        }
                        catch { /* non-essential */ }

                        // Get completion status
                        var completed_node = node.SelectSingleNode(".//div[contains(@class, 'story-status')]//span");
                        if (completed_node != null)
                        {
                            fic.Completed = true;
                        }
                        else
                        {
                            fic.Completed = false;
                        }

                        try
                        {
                            // Get Tags
                            var tag_nodes = node.SelectNodes(".//a[contains(@class, 'tag-item')]");
                            foreach (var tag in tag_nodes)
                            {
                                fic.Tags.Add(new Tuple<string, string>(tag.InnerText, ""));
                            }
                        }
                        catch { /* non-essential */ }

                        //Add fic to final list
                        fics.Add(fic);
                    }
                    catch
                    {
                        continue;
                    }
                }
            }
            
            return fics;
        }

        private bool ConvertStringMetaNumbers(string meta, out ulong num)
        {
            if (ulong.TryParse(meta, out ulong likes))
            {
                num = likes;
                return true;
            }
            else
            {
                foreach (char id in NumConversions.Keys)
                {
                    if (meta.EndsWith(id))
                    {
                        meta = meta.Trim(id);
                        if (float.TryParse(meta, out float likes_mod))
                        {
                            num = Convert.ToUInt64(likes_mod * NumConversions[id]);
                            return true;
                        }
                    }
                }
            }

            num = 0;
            return false;
        }
    }
}
