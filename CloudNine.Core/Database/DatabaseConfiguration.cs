
using Newtonsoft.Json;

namespace CloudNine.Core.Database
{
    public struct DatabaseConfiguration
    {
        [JsonProperty("data_source")]
        public string DataSource { get; set; }
    }
}
