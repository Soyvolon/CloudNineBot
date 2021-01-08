using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

using CloudNine.Core.Http;
using CloudNine.Core.Multisearch.Configuration;

namespace CloudNine.Core.Multisearch
{
    public class MultisearchUser
    {
        [NotMapped]
        public const int CacheSize = 10;

        [Key]
        public ulong Id { get; set; }

        [NotMapped]
        public SearchManager? Manager { get; set; } 

        public List<FanFic> Cache { get; set; }
        public List<Tuple<string, string>> History { get; set; }
        public MultisearchConfigurationOptions Options { get; set; }

        public MultisearchUser()
        {
            Cache = new();
            History = new();
            Options = new();
        }

        /// <summary>
        /// Adds a fanfic to the user Cache.
        /// </summary>
        /// <param name="fic">Fanfic to add</param>
        /// <returns>Fanfic that was removed from the end of the cache.</returns>
        public FanFic? AddToCahce(FanFic fic)
        {
            Cache.Insert(0, fic);
            if(Cache.Count > 10)
            {
                var outFic = Cache[10];
                Cache.RemoveAt(10);
                // Save the title and link to the fic in the history.
                History.Add(new(outFic.Title.Item1, outFic.Title.Item2));
                return outFic;
            }

            return null;
        }

        public async Task<List<FanFic>?> NewSearch(BrowserClient client, Search search, SearchOptions? options = null)
        {
            Manager = new(client, options ?? Options.DefaultSearchOptions);
            await Manager.NewSearch(search);

            // Get the results of the first page.
            if (Manager.TryGetResults(1, out var fics))
                return fics;

            return null;
        }

        public async Task<List<FanFic>?> LoadNextSearchPages()
        {
            if (Manager is not null)
            {
                await Manager.LoadNextSearchPage();
                if (Manager.TryGetCurrentPageResults(out var fics))
                    return fics;
            }

            return null;
        }
    }
}
