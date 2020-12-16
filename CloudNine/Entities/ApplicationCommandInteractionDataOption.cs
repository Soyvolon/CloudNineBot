using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using CloudNine.Enums;

using Newtonsoft.Json;

namespace CloudNine.Entities
{
    public class ApplicationCommandInteractionDataOption
    {
        [JsonProperty("name")]
        public string Name { get; internal set; }
        [JsonProperty("value")]
        public ApplicationCommandOptionType? Type { get; internal set; }
        [JsonProperty("options")]
        public ApplicationCommandInteractionDataOption[]? Options { get; internal set; }
    }
}
