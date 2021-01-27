using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

using CloudNine.Core.Http;
using CloudNine.Core.Multisearch.Configuration;
using CloudNine.Core.Multisearch.Requests;

namespace CloudNine.Core.Multisearch
{
    public class SearchManager : HtmlGrabber
    {
        public ulong User { get; set; }
        public int WebsitePageNumber { get; set; }
        public Search? ActiveSearch { get; private set; }

        private ConcurrentDictionary<int, List<FanFic>> _fanFics { get; set; }

        private readonly SearchOptions _searchOptions;

        public bool TryGetResults(int page, [NotNullWhen(true)] out List<FanFic>? fics)
        {
            if(_fanFics.TryGetValue(page, out fics))
            {
                // TODO: Default sort options.
                fics.Sort((x, y) => x.Likes.CompareTo(y.Likes));
                fics.Reverse();
                return true;
            }

            fics = null;
            return false;
        }

        public bool TryGetCurrentPageResults([NotNullWhen(true)] out List<FanFic>? fics)
            => TryGetResults(WebsitePageNumber, out fics);
        
        public bool TryGetAllResults([NotNullWhen(true)] out List<FanFic>? allFics)
        {
            allFics = new();
            for(int i = 1; i <= WebsitePageNumber; i++)
            {
                if (TryGetResults(i, out var fics))
                    allFics.AddRange(fics);
                else
                {
                    allFics = null;
                    return false;
                }
            }

            return true;
        }

        public SearchManager(FanfictionClient client, SearchOptions options) : base(client)
        {
            _fanFics = new();
            WebsitePageNumber = 1; // start on first page

            _searchOptions = options;
        }

        public async Task NewSearch(Search search)
        {
            ActiveSearch = search; 

            ActiveSearch.BuildRequests(); // Configure the seprate requests

            await MakeWebRequest(); // Make a new web request
        }

        public async Task LoadNextSearchPage()
        {
            WebsitePageNumber++;
            await MakeWebRequest();
        }

        private async Task MakeWebRequest()
        {
            if (ActiveSearch is null)
                throw new ArgumentNullException(nameof(ActiveSearch), "Active Search can not be null");

            foreach (RequestBase request in ActiveSearch.Requests)
            {
                string rstring = request.GetRequestString(WebsitePageNumber);
                if (rstring != null && rstring != "")
                {
                    var html = await GetHtmlAsync(rstring);
                    
                    request.Result.LoadHtml(html);
                    request.FixBasicErrors();

                    if (!_fanFics.ContainsKey(WebsitePageNumber))
                        _ = _fanFics.TryAdd(WebsitePageNumber, new List<FanFic>());

                    _fanFics[WebsitePageNumber].AddRange(request.DecodeHTML(_searchOptions));
                }
            }
        }
    }
}