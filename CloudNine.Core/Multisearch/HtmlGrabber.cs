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
            await page.SetCookieAsync(new PuppeteerSharp.CookieParam[] { new()
                {
                    Name = "view_adult",
                    Value = "true",
                    Domain = "archiveofourown.org",
                    Path = "/",                    
                }
            });
            await page.GoToAsync(url);
            if (url.Contains("archiveofourown.org"))
            {
                await page.EvaluateExpressionAsync("localStorage.setItem('accepted_tos', '20180523');");
                await page.ReloadAsync();
            }
            var html = await page.GetContentAsync();

            return html;
        }
    }
}
