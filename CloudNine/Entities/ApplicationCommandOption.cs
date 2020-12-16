using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using CloudNine.Enums;

using Newtonsoft.Json;

namespace CloudNine.Entities
{
    public class ApplicationCommandOption
    {
        [JsonProperty("type")]
        public ApplicationCommandOptionType Type { get; internal set; }
        [JsonProperty("name")]
        public string Name { get; internal set; }
        [JsonProperty("description")]
        public string Description { get; internal set; }
        [JsonProperty("default")]
        public bool? Default { get; internal set; }
        [JsonProperty("required")]
        public bool? Required { get; internal set; }
        [JsonProperty("choices")]
        public ApplicationCommandOptionChoice[]? Choices { get; internal set; }
        [JsonProperty("options")]
        public ApplicationCommandOption[]? Options { get; internal set; }


        internal ApplicationCommandOption()
        {
            Name = "";
            Description = "";
        }
    }
}
