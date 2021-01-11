using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using CloudNine.Core.Multisearch.Configuration;

namespace CloudNine.Core.Multisearch.Requests
{
    public class WattpadFicRequest : WattpadBase
    {
        private const string work_base = "https://www.wattpad.com/story/";
        private string WorkId { get; init; }
        private string Link { get; init; }

        public WattpadFicRequest(string work_id, string link)
        {
            WorkId = work_id;
            this.Link = link;
        }

        public override string GetRequestString(int pageNumber = 1)
            => work_base + WorkId;

        public override List<FanFic> DecodeHTML(SearchOptions searchOptions)
        {
            List<FanFic> fics = new();

            FanFic fic = new FanFic()
            {
                Site = SiteFrom.Wattpad
            };

            var node = Result.DocumentNode.SelectSingleNode("//header[contains(@class, 'background')]");
            var title_node = node.SelectSingleNode(".//div[contains(@class, 'container')]//img");
            fic.Title = new(title_node.Attributes["alt"].Value.Replace("&#x27;", "'"), Link);

            var author_node = node.SelectSingleNode(".//strong//a[contains(@class, 'send-author-event')]");
            fic.Author = new(author_node.InnerText, link_base + author_node.Attributes["href"].Value);

            var view_fav_chap_nodes = node.SelectNodes(".//div[contains(@class, 'meta')]//span");
            try
            {
                var view_raw = view_fav_chap_nodes[0].Attributes["title"].Value.Split(" ", StringSplitOptions.RemoveEmptyEntries)[0];
                if(string.IsNullOrWhiteSpace(view_raw))
                    view_raw = view_fav_chap_nodes[0].Attributes["data-original-title"].Value.Split(" ", StringSplitOptions.RemoveEmptyEntries)[0];

                fic.Views = Convert.ToUInt64(view_raw.Replace(",", ""));
            }
            catch {  /* non-essential */ }

            try
            {
                var likes_raw = view_fav_chap_nodes[1].Attributes["title"].Value.Split(" ", StringSplitOptions.RemoveEmptyEntries)[0];
                if (string.IsNullOrWhiteSpace(likes_raw))
                    likes_raw = view_fav_chap_nodes[1].Attributes["data-original-title"].Value.Split(" ", StringSplitOptions.RemoveEmptyEntries)[0];
                fic.Likes = Convert.ToUInt64(likes_raw.Replace(",", ""));
            }
            catch {  /* non-essential */ }

            try
            {
                var chapter_raw = view_fav_chap_nodes[2].InnerText.Split(" ", StringSplitOptions.RemoveEmptyEntries)[0];
                fic.Chapters = Convert.ToUInt64(chapter_raw);
            }
            catch {  /* non-essential */ }

            var complete_node = node.SelectSingleNode(".//small//span[contains(@class, 'hidden-xs')]");
            fic.Completed = !complete_node.InnerText.Contains("Ongoing");

            var desc_node = Result.DocumentNode.SelectSingleNode("//h2[contains(@class, 'description')]");
            fic.Description = desc_node.InnerText;

            fics.Add(fic);

            return fics;
        }
    }
}
