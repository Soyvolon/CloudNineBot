using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

using CloudNine.Core.Http;
using CloudNine.Core.Multisearch.Configuration;
using CloudNine.Core.Multisearch.Requests;

namespace CloudNine.Core.Multisearch
{
    public class FanfictionLinkManager : HtmlGrabber
    {
        private readonly List<RequestBase> _requests;
        private readonly SearchOptions _options;

        public FanfictionLinkManager(List<RequestBase> requests, FanfictionClient client, SearchOptions searchOptions) : base(client)
        {
            _requests = requests;
            _options = searchOptions;
        }

        public async Task<List<FanFic>> MakeWebRequest()
        {
            List<FanFic> res = new();

            foreach(var request in _requests)
            {
                var html = await GetHtmlAsync(request.GetRequestString());

                request.Result.LoadHtml(html);
                request.FixBasicErrors();

                res.AddRange(request.DecodeHTML(_options));
            }

            return res;
        }
    }
}
