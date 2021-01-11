using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CloudNine.Core.Multisearch.Requests
{
    public class AO3Base : RequestBase
    {
        protected readonly string link_base = "https://archiveofourown.org";
        public AO3Base() : base() { }
        public AO3Base(Search search) : base(search) { }
    }
}
