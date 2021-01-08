using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CloudNine.Core.Multisearch
{
    public enum CrossoverStatus
    {
        Any = 0,
        NoCrossover,
        Crossover
    }

    public static class CrossoverStatusUtils
    {
        public static string GetString(this CrossoverStatus? dir)
            => dir switch
            {
                CrossoverStatus.Any => "Any",
                CrossoverStatus.NoCrossover => "No Crossovers",
                CrossoverStatus.Crossover => "Crossovers Only",
                _ => "Not Specified"
            };
    }
}
