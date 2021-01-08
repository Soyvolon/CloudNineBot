using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CloudNine.Core.Multisearch
{
    public enum SearchDirection
    {
        Descending = 0,
        Ascending
    }

    public static class SearchDirectionUtils
    {
        public static string GetString(this SearchDirection? dir)
            => dir switch
            {
                SearchDirection.Ascending => "Ascending",
                SearchDirection.Descending => "Descending",
                _ => "Not Specified"
            };
    }
}
