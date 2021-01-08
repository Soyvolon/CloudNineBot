using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CloudNine.Core.Multisearch
{
    public enum SearchBy
    {
        BestMatch = 0,
        Likes,
        Views,
        UpdatedDate,
        PublishedDate,
        Comments
    }

    public static class SearchByUtils
    {
        public static string GetString(this SearchBy? dir)
            => dir switch
            {
                SearchBy.BestMatch => "Best Match",
                SearchBy.Likes => "Likes",
                SearchBy.Views => "Views",
                SearchBy.UpdatedDate => "Updated Date",
                SearchBy.PublishedDate => "Published Date",
                SearchBy.Comments => "Comments",
                _ => "Not Specified"
            };
    }
}
