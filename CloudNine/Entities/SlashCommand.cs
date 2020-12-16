using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CloudNine.Entities
{
    public class SlashCommand
    {
        public Action<Interaction> Execute { get; set; }
    }
}
