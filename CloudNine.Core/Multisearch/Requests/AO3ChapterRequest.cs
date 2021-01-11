using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using CloudNine.Core.Multisearch.Configuration;

namespace CloudNine.Core.Multisearch.Requests
{
    public class AO3ChapterRequest : AO3Base
    {
        private const string chapter_base = "https://archiveofourown.org/chapters/";

        private string ChapterId { get; init; }

        public AO3ChapterRequest(string chapter_id)
        {
            ChapterId = chapter_id;
        }

        public override string GetRequestString(int pageNumber = 1)
        {
            return chapter_base + ChapterId;
        }

        public string GetWorkId()
        {
            var node = Result.DocumentNode.SelectSingleNode("//li[contains(@class, 'chapter')]//noscript//a");
            var dest = node.Attributes["href"].Value;

            dest = dest[(dest.IndexOf("works/") + 6)..];
            dest = dest.Substring(0, dest.IndexOf("/"));

            return dest;
        }
    }
}
