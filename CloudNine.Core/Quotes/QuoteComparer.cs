using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

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

    public class HiddenQuoteComparer : IComparer<Quote>
    {
        public int Compare([AllowNull] Quote x, [AllowNull] Quote y)
        {
            if (x is null && y is null) return 0;
            if (x is null) return -1;
            if (y is null) return 1;

            return x.CustomId.CompareTo(y.CustomId);
        }
    }
}
