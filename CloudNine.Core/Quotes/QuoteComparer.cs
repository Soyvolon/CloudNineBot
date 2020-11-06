using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace CloudNine.Core.Quotes
{
    public class QuoteComparer : IComparer<Quote>
    {
        public int Compare([AllowNull] Quote x, [AllowNull] Quote y)
        {
            if (x is null && y is null) return 0;
            if (x is null) return -1;
            if (y is null) return 1;

            return x.Id.CompareTo(y.Id);
        }
    }
}
