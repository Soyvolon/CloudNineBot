using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using CloudNine.Core.Http;

using RandomUserAgent;

namespace CloudNine.Core.Multisearch
{
    public class HtmlGrabber
    {
        protected readonly BrowserClient _client;

        public HtmlGrabber(BrowserClient client)
        {
            _client = client;
        }

        protected async Task<string> GetHtmlAsync(string url)
        {
            await using var page = await _client.Browser.NewPageAsync();
            await page.SetJavaScriptEnabledAsync(true);
            await page.SetUserAgentAsync(RandomUa.RandomUserAgent);
            await page.GoToAsync(url);
            var html = await page.GetContentAsync();

            return html;
        }
    }
}
