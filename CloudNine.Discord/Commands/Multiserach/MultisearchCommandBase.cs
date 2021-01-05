using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using CloudNine.Core.Multisearch;

using DSharpPlus.CommandsNext.Attributes;

namespace CloudNine.Discord.Commands.Multiserach
{
    [Group("search")]
    public class MultisearchCommandBase : CommandModule
    {
        protected readonly IServiceProvider _services;
        public MultisearchCommandBase(IServiceProvider services)
        {
            this._services = services;
        }
    }
}
