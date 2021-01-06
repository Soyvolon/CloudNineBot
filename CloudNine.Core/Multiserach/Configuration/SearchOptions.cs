using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Newtonsoft.Json;

namespace CloudNine.Core.Multisearch.Configuration
{
    public class SearchOptions
    {
        [JsonProperty("items_per_page")]
        public int ItemsPerPage { get; set; } = 20;
        [JsonProperty("allow_explicit")]
        public bool AllowExplicit { get; set; } = false;
        [JsonProperty("treat_warnings_not_used_as_warnings")]
        public bool TreatWarningsNotUsedAsWarnings { get; set; } = false;
    }
}
