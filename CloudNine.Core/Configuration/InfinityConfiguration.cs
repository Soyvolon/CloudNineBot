using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Newtonsoft.Json;

namespace CloudNine.Core.Configuration
{
    public class InfinityConfiguration
    {
        [JsonProperty("authorized_users")]
        public List<ulong> AuthorizedUsers { get; set; } = new();
        [JsonProperty("google_api_key")]
        public string GoogleApiKey { get; set; } = "";
    }
}
