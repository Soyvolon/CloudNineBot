using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using CloudNine.Core.Multisearch.Configuration;

namespace CloudNine.Core.Multisearch.Requests
{
    public class FanfictionFicRequest : FanfictionBase
    {
        private string Link { get; init; }
        public FanfictionFicRequest(string link) : base()
        {
            Link = link;
        }

        public override string GetRequestString(int pageNumber = 1)
        {
            return Link;
        }

        public override List<FanFic> DecodeHTML(SearchOptions searchOptions)
        {
            List<FanFic> fics = new List<FanFic>();

            var fic = new FanFic()
            {
                Site = SiteFrom.FanfictionDotNet
            };

            var node = Result.DocumentNode.SelectSingleNode("//div[contains(@id, 'profile_top')]");

            var title_node = node.SelectSingleNode(".//b");
            fic.Title = new(title_node.InnerText, Link);
            var author_node = node.SelectSingleNode(".//a[contains(@class, 'xcontrast_txt')]");
            fic.Author = new(author_node.InnerText, link_base + author_node.Attributes["href"].Value);

            var desc_node = node.SelectSingleNode(".//div[contains(@class, 'xcontrast_txt')]");
            fic.Description = desc_node.InnerText;

            var details_node = node.SelectSingleNode(".//span[contains(@class, 'xgray')]");
            var fic_data = details_node.InnerText.Split("-", StringSplitOptions.RemoveEmptyEntries).ToList();
            fic_data.ForEach(x => x = x.Trim()); // removing trailing and leading whitespace

            fic.Rating = fic_data[0];

            fic.Completed = fic_data.Contains("Complete");

            try
            {
                // Get favorites information
                var rating_parts = fic_data.Find(x => x.Contains("Rated"));
                fic_data.Remove(rating_parts);

                var rating_string = rating_parts.Split(" ", StringSplitOptions.RemoveEmptyEntries)[1..];
                fic.Rating = string.Join(" ", rating_string);
            }
            catch
            {
                fic.Rating = "";
            }

            try
            {
                // Get favorites information
                var fav_str = fic_data.Find(x => x.Contains("Favs"));
                fic_data.Remove(fav_str);

                var fav_num = fav_str.Split(" ", StringSplitOptions.RemoveEmptyEntries).Last();
                fav_num = fav_num.Replace(",", "");
                fic.Likes = Convert.ToUInt64(fav_num);
            }
            catch
            {
                fic.Likes = 0;
            }

            try
            {
                // Get words information
                var fav_str = fic_data.Find(x => x.Contains("Words"));
                fic_data.Remove(fav_str);

                var fav_num = fav_str.Split(" ", StringSplitOptions.RemoveEmptyEntries).Last();
                fav_num = fav_num.Replace(",", "");
                fic.WordCount = Convert.ToUInt64(fav_num);
            }
            catch
            {
                fic.WordCount = 0;
            }

            var c = 0;
            while (!fic_data[c].Contains(":"))
                fic.AddTag(new(fic_data[c++], ""));

            fics.Add(fic);

            return fics;
        }
    }
}
