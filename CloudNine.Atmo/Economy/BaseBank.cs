using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Newtonsoft.Json;

namespace CloudNine.Atmo.Economy
{
    public class BaseBank
    {
        [JsonProperty("raindrops")]
        public int Raindrops { get; internal set; }
        [JsonProperty("cloud")]
        public int Cloud { get; internal set; }
        [JsonProperty("Static")]
        public int Static { get; internal set; }
        [JsonProperty("Stars")]
        public int Stars { get; internal set; }

        // TODO: Deposit, Withdraw, Pay, Gift, and Convert methods.
    }
}
