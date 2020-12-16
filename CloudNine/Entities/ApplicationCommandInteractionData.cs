using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Newtonsoft.Json;

namespace CloudNine.Entities
{
    public class ApplicationCommandInteractionData
    {
        [JsonProperty("id")]
        public ulong Id { get; internal set; }
        [JsonProperty("name")]
        public string Name { get; internal set; }
        [JsonProperty("options")]
        public ApplicationCommandInteractionDataOption[]? Options { get; internal set; }
    }
}
