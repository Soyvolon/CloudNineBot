using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CloudNine.Commands
{
    public class SlashCommandBase
    {
        protected readonly IServiceProvider _services;

        public SlashCommandBase(IServiceProvider services)
        {
            _services = services;
        }
    }
}
