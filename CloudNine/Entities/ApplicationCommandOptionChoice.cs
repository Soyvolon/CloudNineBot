using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Newtonsoft.Json;

namespace CloudNine.Entities
{
    public class ApplicationCommandOptionChoice
    {
        [JsonProperty("name")]
        public string Name { get; internal set; }

        /// <summary>
        /// Must be string or int
        /// </summary>
        [JsonProperty("value")]
        public object Value { get; internal set; }

        public ApplicationCommandOptionChoice(string n, int v)
        {
            Name = n;
            Value = v;
        }

        public ApplicationCommandOptionChoice(string n, string v)
        {
            Name = n;
            Value = v;
        }
    }
}
