using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Newtonsoft.Json;

namespace CloudNine.Atmo.State
{
    public abstract class BaseState
    {
        [JsonIgnore]
        public int State { get; internal set; } = 0;
    }
}
