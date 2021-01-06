using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

using CloudNine.Core.Http;
using CloudNine.Core.Multisearch.Builders;
using CloudNine.Core.Multisearch.Configuration;
using CloudNine.Core.Multisearch.Requests;

using PuppeteerSharp;

namespace CloudNine.Core.Multisearch
{
    public class SearchManager
    {
        public ulong User { get; set; }
        public int ItemsPerPage { get; set; }
        public int WebsitePageNumber { get; set; }
        public Search? ActiveSearch { get; private set; }

        private ConcurrentDictionary<int, List<FanFic>> _fanFics { get; set; }
        private readonly BrowserClient _client;

        private readonly SearchOptions _searchOptions;

        public bool TryGetResults(int page, out List<FanFic>? fics)
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

        public bool TryGetCurrentPageResults(out List<FanFic>? fics)
            => TryGetResults(WebsitePageNumber, out fics);
        

        public SearchManager(BrowserClient client, SearchOptions options)
        {
            _fanFics = new();
            WebsitePageNumber = 1; // start on first page
            ItemsPerPage = options.ItemsPerPage;


            _searchOptions = options;
            _client = client;
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
                    var html = await GetHtml(rstring);
                    
                    request.Result.LoadHtml(html);
                    request.FixBasicErrors();

                    if (!_fanFics.ContainsKey(WebsitePageNumber))
                        _ = _fanFics.TryAdd(WebsitePageNumber, new List<FanFic>());

                    _fanFics[WebsitePageNumber].AddRange(request.DecodeHTML(_searchOptions));
                }
            }
        }

        private async Task<string> GetHtml(string url)
        {
            await using var page = await _client.Browser.NewPageAsync();
            await page.GoToAsync(url);
            var html = await page.GetContentAsync();
            
            return html;
        }
    }
}