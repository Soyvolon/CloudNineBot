using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CloudNine.Core.Multisearch.Requests
{
    public class WattpadBase : RequestBase
    {
        protected readonly Dictionary<char, long> NumConversions = new Dictionary<char, long> { { 'M', 1000000 }, { 'K', 1000 } };
        protected const string link_base = "https://wattpad.com";
        public WattpadBase() : base() { }
        public WattpadBase(Search search) : base(search) { }
    }
}
