using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CloudNine.Core.Moderation
{
    public class CustomDateTimeComparer : IComparer<DateTime>
    {
        public int Compare(DateTime x, DateTime y)
        {
            // Invert the comparison
            return -x.CompareTo(y);
        }
    }
}
