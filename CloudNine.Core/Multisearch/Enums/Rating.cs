using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CloudNine.Core.Multisearch
{

    public enum Rating
    {
        Any = 0,
        General,
        Teen,
        Mature,
        Explicit,
        NotExplicit
    }

    public static class RatingUtils
    {
        public static string GetString(this Rating? dir)
            => dir switch
            {
                Rating.Any => "Any",
                Rating.General => "General",
                Rating.Teen => "Teen",
                Rating.Mature => "Mature",
                Rating.Explicit => "Explicit",
                Rating.NotExplicit => "Not Explicit",
                _ => "Not Specified"
            };
    }
}
