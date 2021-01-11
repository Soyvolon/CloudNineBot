using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CloudNine.Core.Multisearch.Requests
{
    public class WattpadChapterRequest : WattpadBase
    {
        private const string chapter_base = "https://www.wattpad.com/";

        private string ChapterId { get; init; }

        public WattpadChapterRequest(string chapter_id)
        {
            ChapterId = chapter_id;
        }

        public override string GetRequestString(int pageNumber = 1)
        {
            return chapter_base + ChapterId;
        }

        public string GetWorkId()
        {
            var node = Result.DocumentNode.SelectSingleNode("//div[contains(@class, 'toc-header')]//a[contains(@class, 'on-navigate')]");
            var dest = node.Attributes["href"].Value;

            dest = dest[(dest.IndexOf("story/") + 6)..];
            dest = dest.Substring(0, dest.IndexOf("-"));

            return dest;
        }
    }
}
