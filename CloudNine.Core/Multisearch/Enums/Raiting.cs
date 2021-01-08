using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CloudNine.Core.Multisearch
{

    public enum Raiting
    {
        Any = 0,
        General,
        Teen,
        Mature,
        Explicit,
        NotExplicit
    }

    public static class RaitingUtils
    {
        public static string GetString(this Raiting? dir)
            => dir switch
            {
                Raiting.Any => "Any",
                Raiting.General => "General",
                Raiting.Teen => "Teen",
                Raiting.Mature => "Mature",
                Raiting.Explicit => "Explicit",
                Raiting.NotExplicit => "Not Explicit",
                _ => "Not Specified"
            };
    }
}
