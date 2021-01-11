using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using CloudNine.Core.Multisearch.Configuration;

namespace CloudNine.Core.Multisearch.Requests
{
    public class AO3FicRequest : AO3Base
    {
        private const string fic_base = "https://archiveofourown.org/works/";
        private const string explicit_string = "Explicit";
        private const string no_archive_warnings = "No Archive Warnings Apply";
        private const string create_chose_no_warnings = "Creator Chose Not To Use Archive Warnings";

        private string FicId { get; init; }
        private string Link { get; init; }

        public AO3FicRequest(string fic_id, string link) : base()
        {
            this.FicId = fic_id;
            this.Link = link;
        }

        public override string GetRequestString(int pageNumber = 1)
        {
            return fic_base + FicId;
        }

        public override List<FanFic> DecodeHTML(SearchOptions searchOptions)
        {
            List<FanFic> fics = new();

            var node = Result.DocumentNode.SelectSingleNode("//div[contains(@class, 'work')]");
            try
            {
                FanFic fic = new FanFic()
                {
                    Site = Multisearch.SiteFrom.ArchiveOfOurOwn
                };

                // Gather story information
                var title_node = node.SelectSingleNode(".//div[contains(@class, 'preface')]//h2[contains(@class, 'title')]");
                fic.Title = new Tuple<string, string>(title_node.InnerText.Trim(), Link);
                var author_node = node.SelectSingleNode(".//div[contains(@class, 'preface')]//h3[contains(@class, 'byline')]//a");
                fic.Author = new Tuple<string, string>(author_node.InnerText.Trim(), link_base + author_node.Attributes["href"].Value);

                try // non-essential
                {
                    // Get fandom information
                    var fandom_node = node.SelectSingleNode(".//dd[contains(@class, 'fandom')]");
                    foreach (var fandom in fandom_node.SelectNodes(".//a"))
                    {
                        fic.Fandoms.Add(new Tuple<string, string>(fandom.InnerText, link_base + fandom.Attributes["href"].Value));
                    }
                }
                catch { /* non-essential */ }

                try
                {
                    var required_tags = node.SelectSingleNode(".//div[contains(@class, 'wrapper')]");
                    var rating = required_tags.SelectSingleNode(".//dd[contains(@class, 'rating')]");
                    fic.IsExplicit = rating?.InnerText.Trim().Equals(explicit_string) ?? false;
                    if (fic.IsExplicit && !searchOptions.AllowExplicit)
                        return fics; // we dont want this fic, it is explicit and we have that diabled.

                    fic.Rating = rating?.InnerText.Trim() ?? "";

                    var complete = required_tags.SelectSingleNode(".//dl[contains(@class, 'stats')]//dd[contains(@class, 'chapters')]");
                    if (complete is not null)
                    {
                        var text = complete.InnerText.Split("/");
                        if (text.Length != 2)
                            fic.Completed = false;
                        else
                        {
                            if (uint.TryParse(text[0], out var first))
                            {
                                if (uint.TryParse(text[1], out var second))
                                {
                                    fic.Completed = first == second;
                                }
                                else fic.Completed = false;

                                fic.Chapters = first;
                            }
                            else fic.Completed = false;
                        }
                    }
                    else
                        fic.Completed = false;

                    var warnings = required_tags.SelectSingleNode(".//dd[contains(@class, 'warning')]//a[contains(@class, 'tag')]");
                    if (warnings is not null)
                    {
                        foreach (var w in warnings.ChildNodes)
                        {
                            fic.SensitiveContentWarning = !w.InnerText.Equals(no_archive_warnings)
                                || w.InnerText.Equals(create_chose_no_warnings) && searchOptions.TreatWarningsNotUsedAsWarnings;

                            if (fic.SensitiveContentWarning)
                                break;
                        }
                    }
                    else
                    {
                        fic.SensitiveContentWarning = searchOptions.TreatWarningsNotUsedAsWarnings;
                    }
                }
                catch { /* non-essential */ }

                try // non-essential
                {
                    var r_tag_nodes = node.SelectSingleNode(".//dd[contains(@class, 'relationship')]");
                    if (r_tag_nodes is not null)
                    {
                        foreach (var tag in r_tag_nodes.SelectNodes(".//a[contains(@class, 'tag')]"))
                            fic.AddRelationshipTag(new(tag.InnerText, link_base + tag.Attributes["href"].Value));
                    }

                    var c_tag_nodes = node.SelectSingleNode(".//dd[contains(@class, 'character')]");
                    if (c_tag_nodes is not null)
                    {
                        foreach (var tag in c_tag_nodes.SelectNodes(".//a[contains(@class, 'tag')]"))
                            fic.AddCharacterTag(new(tag.InnerText, link_base + tag.Attributes["href"].Value));
                    }

                    var tag_nodes = node.SelectSingleNode(".//dd[contains(@class, 'freeform')]");
                    if (tag_nodes is not null)
                    {
                        foreach (var tag in tag_nodes.SelectNodes(".//a[contains(@class, 'tag')]"))
                            fic.AddTag(new(tag.InnerText, link_base + tag.Attributes["href"].Value));
                    }
                }
                catch { /* non-essential */ }

                try // non-essential
                {
                    // Get fic description
                    var fic_desc = node.SelectSingleNode(".//blockquote[contains(@class, 'userstuff')]");
                    fic.Description = fic_desc.InnerText;
                }
                catch { /* non-essential */ }

                try
                {
                    var fic_likes = node.SelectSingleNode(".//dl[contains(@class, 'stats')]//dd[contains(@class, 'kudos')]");
                    fic.Likes = Convert.ToUInt64(fic_likes.InnerText);
                }
                catch { /* non-essential */ }

                try
                {
                    var fic_hits = node.SelectSingleNode(".//dl[contains(@class, 'stats')]//dd[contains(@class, 'hits')]");
                    fic.Views = Convert.ToUInt64(fic_hits.InnerText);
                }
                catch { /* non-essential */ }

                try
                {
                    var fic_words = node.SelectSingleNode(".//dl[contains(@class, 'stats')]//dd[contains(@class, 'words')]");
                    fic.WordCount = Convert.ToUInt64(fic_words.InnerText);
                }
                catch { /* non-essential */ }

                fics.Add(fic);
            }
            catch
            {
                 /* do nothing, we are not returning any fics. */
            }

            return fics;
        }
    }
}
