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

        public MultisearchCache Cache { get; set; }
        
        public MultisearchConfigurationOptions Options { get; set; }

        public MultisearchUser()
        {
            Options = new();
            Cache = new();
        }

        public async Task<List<FanFic>?> NewSearch(FanfictionClient client, Search search, SearchOptions? options = null)
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
