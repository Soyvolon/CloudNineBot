using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using DSharpPlus.SlashCommands.Attributes;
using DSharpPlus.SlashCommands.Entities;

namespace CloudNine.Commands
{
    public class GetRandomQuoteCommand : BaseSlashCommandModule
    {
        public GetRandomQuoteCommand(IServiceProvider s) : base(s) { }

        [SlashCommand("quote", 1, 750486424469372970)]
        public async Task GetRandomQuoteCommandAsync(InteractionContext ctx)
        {

        }
    }
}
