using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using CloudNine.Core.Multisearch;

using Newtonsoft.Json;

namespace CloudNine.Core.Multisearch.Configuration
{
    public class SearchConfiguration
    {
        [JsonProperty("direction")]
        public SearchDirection? Direction { get; set; } = SearchDirection.Descending;
        [JsonProperty("search_fics_by")]
        public SearchBy? SearchFicsBy { get; set; } = SearchBy.BestMatch;
        [JsonProperty("fic_rating")]
        public Rating? FicRating { get; set; } = Rating.NotExplicit;
        [JsonProperty("status")]
        public FicStatus? Status { get; set; } = FicStatus.Any;
        [JsonProperty("crossover")]
        public CrossoverStatus? Crossover { get; set; } = CrossoverStatus.Any;
    }
}
