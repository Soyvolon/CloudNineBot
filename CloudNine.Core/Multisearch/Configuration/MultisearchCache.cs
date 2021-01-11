using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Newtonsoft.Json;

namespace CloudNine.Core.Multisearch.Configuration
{
    public class MultisearchCache
    {
        [JsonProperty("cache")]
        public List<FanFic> Cache { get; set; }
        [JsonProperty("history")]
        public List<Tuple<string, string>> History { get; set; }

        public MultisearchCache()
        {
            Cache = new();
            History = new();
        }

        /// <summary>
        /// Adds a fanfic to the user Cache.
        /// </summary>
        /// <param name="fic">Fanfic to add</param>
        /// <returns>Fanfic that was removed from the end of the cache.</returns>
        public FanFic? AddToCahce(FanFic fic)
        {
            Cache.Insert(0, fic);
            if (Cache.Count > 10)
            {
                var outFic = Cache[10];
                Cache.RemoveAt(10);
                // Save the title and link to the fic in the history.
                History.Add(new(outFic.Title.Item1, outFic.Title.Item2));
                return outFic;
            }

            return null;
        }

        public bool TryGetValue(int item, [NotNullWhen(true)] out object? value)
        {
            if(item < 10)
            {
                if(Cache.Count > item)
                {
                    value = Cache[item];
                    return true;
                }

                value = null;
                return false;
            }
            else
            {
                item -= 10;

                if(History.Count > item)
                {
                    value = History[item];
                    return true;
                }

                value = null;
                return false;
            }
        }
    }
}
