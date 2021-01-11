using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CloudNine.Core.Multisearch.Requests
{
    public class FanfictionBase : RequestBase
    {
        protected readonly string link_base = "https://fanfiction.net";
        public FanfictionBase() : base() { }
        public FanfictionBase(Search search) : base(search) { }
    }
}
