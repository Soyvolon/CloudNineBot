using System;
using System.Collections.Generic;

using CloudNine.Core.Multisearch.Configuration;

using HtmlAgilityPack;

namespace CloudNine.Core.Multisearch.Requests
{
    /// <summary>
    /// Holds data to send a GET request to Archiveofourown.org and information to decompile the HTML script into a FanFic object
    /// </summary>
    public class AO3Request : AO3Base
    {

        //TODO: Expand search results to include more information than just a basic search and page number
        private const string request_body = "https://archiveofourown.org/works/search?utf8=%E2%9C%93";
        private const string request_part_basic = "work_search%5Bquery%5D=";
        private const string request_part_title = "work_search%5Btitle%5D=";

        private const string request_part_authors = "work_search%5Bcreators%5D=";
        private const string request_part_characters = "work_search%5Bcharacter_names%5D=";
        private const string request_part_relationships = "work_search%5Brelationship_names%5D=";
        private const string request_part_fandoms = "work_search%5Bfandom_names%5D=";
        private const string request_part_other_tags = "work_search%5Bfreeform_names%5D=";

        private const string request_part_likes = "work_search%5Bkudos_count%5D=";
        private const string request_part_views = "work_search%5Bhits%5D=";
        private const string request_part_comments = "work_search%5Bcomments_count%5D=";
        private const string request_part_word_count = "work_search%5Bword_count%5D";

        private const string request_part_update_before = "work_search%5Brevised_at%5D=";

        private const string request_part_sort_by = "work_search%5Bsort_column%5D=";
        private const string request_part_sort_dir = "work_search%5Bsort_direction%5D=";
        private const string request_part_rating = "work_search%5Brating_ids%5D=";
        private const string request_part_status = "work_search%5Bcomplete%5D=";
        private const string request_part_crossover = "work_search%5Bcrossover%5D=";

        private readonly string page_request_body = "page=";

        public AO3Request() : base()
        {

        }

        public AO3Request(Search search) : base(search)
        {
            // Archive specefic features
            Authors = search.Authors;
            Characters = search.Characters;
            Relationships = search.Relationships;
            Fandoms = search.Fandoms;
            OtherTags = search.OtherTags;
                        
            Likes = GetDualNumberString(search.Likes);
            Views = GetDualNumberString(search.Views);
            Comments = GetDualNumberString(search.Comments);
            WordCount = GetDualNumberString(search.WordCount);

            UpdateBefore = GetDualTimeString(search.UpdateBefore);

            switch (search.SearchFicsBy)
            {
                case SearchBy.BestMatch:
                    SortBy = "_score";
                    break;
                case SearchBy.Comments:
                    SortBy = "comments_count";
                    break;
                case SearchBy.Likes:
                    SortBy = "kudos_count";
                    break;
                case SearchBy.Views:
                    SortBy = "hits";
                    break;
                case SearchBy.UpdatedDate:
                    SortBy = "revised_at";
                    break;
                case SearchBy.PublishedDate:
                    SortBy = "created_at";
                    break;
                case null:
                    SortBy = null;
                    break;
            }

            switch(search.Direction)
            {
                case SearchDirection.Descending:
                    SortDir = "desc";
                    break;
                case SearchDirection.Ascending:
                    SortDir = "asc";
                    break;
                case null:
                    SortDir = null;
                    break;
            }
        }

        private string? GetDualNumberString(Tuple<int, int>? value)
        {
            if (value is null)
                return null;

            var first = value.Item1;
            var last = value.Item2;

            if (first == last)
                return first.ToString();
            else if (first == 0)
                return $"<{last}";
            else if (last == 0)
                return $">{first}";
            else if (first < last)
                return $"{first}-{last}";
            else
                return null;
        }

        private string? GetDualTimeString(Tuple<DateTime, DateTime>? value)
        {
            if (value is null)
                return null;

            try
            {
                var now = DateTime.Now;
                var first = value.Item1 == DateTime.MinValue ? 0 : Convert.ToInt32((now - value.Item1).TotalDays);
                var last = value.Item2 == DateTime.MinValue ? 0 : Convert.ToInt32((now - value.Item2).TotalDays);

                if (first == last)
                    return $"{first} days";
                else if (first == 0)
                    return $"<{last} days";
                else if (last == 0)
                    return $">{first} days";
                else if (first < last)
                    return $"{first}-{last} days";
                else
                    return null;
            }
            catch
            {
                return null;
            }
        }

        public override string GetRequestString(int pageNumber = 1)
        {
            // Alway use the basic request, even if it is blank.
            string request = $"{request_body}&{request_part_basic}{Query}";

            if (Title is not null)
                request += $"&{request_part_title}{Title}";

            if (Authors is not null)
            {
                if (Authors.Count > 0)
                {
                    request += $"&{request_part_authors}{Authors[0]}";

                    for (int i = 1; i < Authors.Count; i++)
                        request += $"%2C{Authors[i]}";
                }
            }
            if (Characters is not null)
            {
                if (Characters.Count > 0)
                {
                    request += $"&{request_part_characters}{Characters[0]}";

                    for (int i = 1; i < Characters.Count; i++)
                        request += $"%2C{Characters[i]}";
                }
            }
            if (Relationships is not null)
            {
                if (Relationships.Count > 0)
                {
                    request += $"&{request_part_relationships}{Relationships[0]}";

                    for (int i = 1; i < Relationships.Count; i++)
                        request += $"%2C{Relationships[i]}";
                }
            }
            if (Fandoms is not null)
            {
                if (Fandoms.Count > 0)
                {
                    request += $"&{request_part_fandoms}{Fandoms[0]}";

                    for (int i = 1; i < Fandoms.Count; i++)
                        request += $"%2C{Fandoms[i]}";
                }
            }
            if (OtherTags is not null)
            {
                if (OtherTags.Count > 0)
                {
                    request += $"&{request_part_other_tags}{OtherTags[0]}";

                    for (int i = 1; i < OtherTags.Count; i++)
                        request += $"%2C{OtherTags[i]}";
                }
            }

            if (Likes is not null)
                request += $"&{request_part_likes}{Likes}";
            if (Views is not null)
                request += $"&{request_part_views}{Views}";
            if (Comments is not null)
                request += $"&{request_part_comments}{Comments}";
            if (WordCount is not null)
                request += $"&{request_part_word_count}{WordCount}";

            if (UpdateBefore is not null)
                request += $"&{request_part_update_before}{UpdateBefore}";

            if (SortDir is not null)
                request += $"&{request_part_sort_dir}{SortDir}";
            if (SortBy is not null)
                request += $"&{request_part_sort_by}{SortBy}";
            if (Rating is not null)
                request += $"&{request_part_rating}{Rating}";
            if (Status is not null)
                request += $"&{request_part_status}{Status}";
            if (Crossover is not null)
                request += $"&{request_part_crossover}{Crossover}";

            request += $"&{page_request_body}{pageNumber}";

            return request;
        }
        /// <summary>
        /// Breaks apart the result of the serach and reurns a list of FanFic that were found in the result
        /// </summary>
        /// <returns></returns>
        public override List<FanFic> DecodeHTML(SearchOptions searchOptions)
        {
            var nodes = Result.DocumentNode.SelectNodes("//li[contains(@class, 'work')]");

            List<FanFic> fics = new List<FanFic>();

            if (nodes != null)
            {
                foreach (HtmlNode node in nodes)
                { // TODO: Determine error problems when reading information
                    try
                    {
                        FanFic fic = new FanFic()
                        { 
                            Site = Multisearch.SiteFrom.ArchiveOfOurOwn
                        };

                        // Gather story information
                        var header_nodes = node.SelectNodes(".//h4[contains(@class, 'heading')]//a");
                        fic.Title = new Tuple<string, string>(header_nodes[0].InnerText, link_base + header_nodes[0].Attributes["href"].Value);
                        fic.Author = new Tuple<string, string>(header_nodes[1].InnerText, link_base + header_nodes[1].Attributes["href"].Value);

                        try // non-essential
                        {
                            // Get fandom information
                            var fandom_node = node.SelectSingleNode(".//h5[contains(@class, 'fandoms')]");
                            foreach (var fandom in fandom_node.SelectNodes(".//a"))
                            {
                                fic.Fandoms.Add(new Tuple<string, string>(fandom.InnerText, link_base + fandom.Attributes["href"].Value));
                            }
                        }
                        catch { /* non-essential */ }

                        try
                        {
                            var required_tags = node.SelectSingleNode(".//ul[contains(@class, 'required-tags')]");
                            var rating = required_tags.SelectSingleNode(".//li//span[contains(@class, 'rating')]");
                            fic.IsExplicit = rating.HasClass("rating-explicit");
                            if (fic.IsExplicit && !searchOptions.AllowExplicit)
                                continue; // we dont want this fic, it is explicit and we have that diabled.

                            fic.Rating = rating?.InnerText ?? "";

                            var complete = required_tags.SelectSingleNode(".//li//span[contains(@class, 'iswip')]//span[contains(@class, 'text')]");
                            if (complete is not null)
                                fic.Completed = complete.InnerText == "Complete Work";
                            else
                                fic.Completed = false;

                            var warnings = required_tags.SelectSingleNode(".//li//span[contains(@class, 'warnings')]");
                            fic.SensitiveContentWarning = !(warnings.HasClass("warning-no") 
                                || (warnings.HasClass("warning-choosenotto") && !searchOptions.TreatWarningsNotUsedAsWarnings));
                        }
                        catch { /* non-essential */ }

                        try // non-essential
                        {
                            // Get tag information
                            var tag_nodes = node.SelectNodes(".//ul[contains(@class, 'tags')]//li");
                            foreach (var tag_node in tag_nodes)
                            {
                                var tag = tag_node.SelectSingleNode(".//a");
                                if (tag_node.HasClass("characters"))
                                    fic.AddCharacterTag(new(tag.InnerText, link_base + tag.Attributes["href"].Value));
                                else if (tag_node.HasClass("relationships"))
                                    fic.AddRelationshipTag(new(tag.InnerText, link_base + tag.Attributes["href"].Value));
                                else
                                    fic.AddTag(new Tuple<string, string>(tag.InnerText, link_base + tag.Attributes["href"].Value));
                            }
                        }
                        catch { /* non-essential */ }

                        try // non-essential
                        {
                            // Get fic description
                            var fic_desc = node.SelectSingleNode(".//blockquote[contains(@class, 'summary')]");
                            fic.Description = fic_desc.InnerText;
                        }
                        catch { /* non-essential */ }

                        try
                        {
                            var fic_likes = node.SelectSingleNode(".//dd[contains(@class, 'kudos')]");
                            fic.Likes = Convert.ToUInt64(fic_likes.InnerText);
                        }
                        catch { /* non-essential */ }

                        try
                        {
                            var fic_hits = node.SelectSingleNode(".//dd[contains(@class, 'hits')]");
                            fic.Views = Convert.ToUInt64(fic_hits.InnerText);
                        }
                        catch { /* non-essential */ }

                        fics.Add(fic);
                    }
                    catch
                    {
                        continue; //TODO: Fix the error catching later
                    }
                }
            }

            return fics;
        }
    }
}