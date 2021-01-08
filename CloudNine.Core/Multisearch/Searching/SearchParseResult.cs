using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using CloudNine.Core.Multisearch.Builders;
using CloudNine.Core.Multisearch.Configuration;

namespace CloudNine.Core.Multisearch.Searching
{
    public class SearchParseResult
    {
        public bool DisplayHelp { get; set; }
        public bool Errored { get; set; }
        public SearchOptions? SearchOptions { get; set; }
        public string? ErrorMessage { get; set; }
        public SearchBuilder? SearchBuilder { get; set; }
    }
}
