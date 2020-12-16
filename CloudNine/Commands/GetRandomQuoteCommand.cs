using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using CloudNine.Core.Database;
using CloudNine.Entities;

namespace CloudNine.Commands
{
    public class GetRandomQuoteCommand : SlashCommandBase
    {
        private readonly IServiceProvider _services;

        public GetRandomQuoteCommand(IServiceProvider services)
        {
            _services = services;
        }

        [SlashCommand("quote", 1, 750486424469372970)]
        public async Task GetRandomQuoteCommandAsync(Interaction interact)
        {

        }
    }
}
