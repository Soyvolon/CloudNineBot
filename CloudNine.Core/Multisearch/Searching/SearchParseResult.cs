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
        public bool DisplayHelp { get; internal set; }
        public bool Errored { get; internal set; }
        public SearchOptions? SearchOptions { get; internal set; }
        public string? ErrorMessage { get; internal set; }
        public SearchBuilder? SearchBuilder { get; internal set; }
    }
}
