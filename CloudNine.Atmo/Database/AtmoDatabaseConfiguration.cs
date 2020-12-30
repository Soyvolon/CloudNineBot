using Newtonsoft.Json;

namespace CloudNine.Atmo.Database
{
    internal class AtmoDatabaseConfiguration
    {
        [JsonProperty("atmo_data_source")]
        internal string DataSource { get; set; }
    }
}