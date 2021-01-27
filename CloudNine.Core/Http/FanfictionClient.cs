using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace CloudNine.Core.Http
{
    public class FanfictionClient
    {
        public HttpClient Client { get; set; }

        public FanfictionClient(HttpClient client)
            => Client = client; // setup done elsewhere.
    }
}
