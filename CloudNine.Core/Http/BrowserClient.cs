using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using PuppeteerSharp;

namespace CloudNine.Core.Http
{
    public class BrowserClient
    {
        public class NotInitializedException : Exception
        {
            public NotInitializedException() : base() { }
            public NotInitializedException(string? message) : base(message) { }
        }

        public Browser Browser
        {
            get
            {
                if (_browser is null) // this will be only called once.
                    Initialize().GetAwaiter().GetResult();

#pragma warning disable CS8603 // Possible null reference return. -- object built in Initalize()
                return _browser;
#pragma warning restore CS8603 // Possible null reference return.
            }
        }

        public Browser? _browser { get; set; }

        public BrowserClient()
        {

        }

        public async Task Initialize()
        {
            await new BrowserFetcher().DownloadAsync(BrowserFetcher.DefaultRevision);
            _browser = await Puppeteer.LaunchAsync(new LaunchOptions
            {
                Headless = true
            });
        }
    }
}
