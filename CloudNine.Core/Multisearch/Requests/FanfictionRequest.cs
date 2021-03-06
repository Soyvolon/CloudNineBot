﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using CloudNine.Core.Multisearch.Configuration;

namespace CloudNine.Core.Multisearch.Requests
{
    public class FanfictionRequest : FanfictionBase
    {
        //TODO: Expand search results to include more information than just a basic search and page number
        private const string request_body = "https://www.fanfiction.net/search/?ready=1";
        private const string type_body = "&type=";
        private const string keywords_body = "&keywords=";
        private const string match_body = "&match=";
        private const string sort_by_body = "&sort=";
        private const string crossover_body = "&typeid=";

        public string Type { get; set; }
        public string Match { get; set; }
        
        public FanfictionRequest() : base()
        {
            Type = "story"; // defaults to a story serach
        }

        public FanfictionRequest(Search search) : base(search)
        {
            Type = "story";

            if(search.Title is not null && search.Basic is null)
            {
                Query = search.Title;
                Match = "title";
            }
            else
            {
                Match = "any";
            }

            Crossover = search.Crossover switch
            {
                CrossoverStatus.Any => "any",
                CrossoverStatus.Crossover => "1",
                CrossoverStatus.NoCrossover => "2",
                _ => "any"
            };

            SortBy = search.SearchFicsBy switch
            {
                SearchBy.BestMatch => "0",
                SearchBy.UpdatedDate => "dateupdate",
                SearchBy.PublishedDate => "datesubmit",
                _ => "0"
            };
        }
        public override string GetRequestString(int pageNumber = 1)
        {
            return request_body + keywords_body + Query + type_body + Type + match_body 
                + Match + sort_by_body + SortBy + crossover_body + Crossover;
        }

        public override List<FanFic> DecodeHTML(SearchOptions searchOptions)
        {
            List<FanFic> fics = new List<FanFic>();

            var nodes = Result.DocumentNode.SelectNodes("//div[contains(@class, 'z-list')]");

            if (nodes != null)
            {
                foreach (var node in nodes)
                {
                    FanFic fic = new FanFic()
                    {
                        Site = Multisearch.SiteFrom.FanfictionDotNet,
                        SensitiveContentWarning = searchOptions.TreatWarningsNotUsedAsWarnings // ff.net has no warnings sections.
                    };

                    try
                    {
                        // Get title info
                        var title_node = node.SelectSingleNode(".//a[contains(@class, 'stitle')]");
                        fic.Title = new Tuple<string, string>(title_node.InnerText, link_base + title_node.Attributes["href"].Value);

                        // Get Author info
                        var author_node = node.SelectNodes(".//a")[1]; // Get author info
                        fic.Author = new Tuple<string, string>(author_node.InnerText, author_node.Attributes["href"].Value);

                        // Get fic info
                        var data_node = node.SelectSingleNode(".//div[contains(@class, 'z-padtop2')]");
                        var fic_data = data_node.InnerText.Split("-", StringSplitOptions.RemoveEmptyEntries).ToList();
                        fic_data.ForEach(x => x = x.Trim()); // removing trailing and leading whitespace

                        fic.Fandoms.Add(new Tuple<string, string>(fic_data[0], ""));

                        // Find tag information
                        if (fic_data.Last().Contains("Complete") && fic_data.FindIndex(x => x.Contains("Published")) != fic_data.Count - 2)
                        {
                            // Run if the last item is complete and second to last is not Published date -- means there are tags
                            fic.Tags.Add(new Tuple<string, string>(fic_data[fic_data.Count - 2], ""));
                        }
                        else if (!fic_data.Last().Contains("Published") && !fic_data.Last().Contains("Complete"))
                        {
                            // Run if the last item is not published and the fic is not complete
                            fic.Tags.Add(new Tuple<string, string>(fic_data.Last(), ""));
                        }

                        try
                        {
                            // Get favorites information
                            var fav_str = fic_data.Find(x => x.Contains("Favs"));
                            var fav_num = fav_str.Split(" ", StringSplitOptions.RemoveEmptyEntries).Last();
                            fav_num = fav_num.Replace(",", "");
                            fic.Likes = Convert.ToUInt64(fav_num);
                        }
                        catch
                        {
                            fic.Likes = 0;
                        }

                        // Get description info
                        var desc_data = node.SelectSingleNode(".//div[contains(@class, 'z-padtop')]");
                        fic.Description = desc_data.InnerText.Replace(data_node.InnerText, "");

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

    }
}
