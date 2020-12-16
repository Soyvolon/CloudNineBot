using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using CloudNine.Attributes;
using CloudNine.Core.Database;
using CloudNine.Entities;

namespace CloudNine.Commands
{
    public class GetRandomQuoteCommand : SlashCommandBase
    {
        public GetRandomQuoteCommand(IServiceProvider s) : base(s) { }

        [SlashCommand("quote", 1, 750486424469372970)]
        public async Task GetRandomQuoteCommandAsync(Interaction interact)
        {

        }
    }
}
