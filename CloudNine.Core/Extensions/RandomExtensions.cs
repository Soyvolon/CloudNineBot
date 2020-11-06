using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CloudNine.Core.Extensions
{
    public static class RandomExtensions
    {
        private static readonly Random _rand = new Random();

        public static T Random<T>(this HashSet<T> set)
        {
            lock (set)
            {
                return set.ElementAt(_rand.Next(set.Count));
            }
        }
    }
}
