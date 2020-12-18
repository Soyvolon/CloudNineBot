using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using DSharpPlus.SlashCommands.Attributes;
using DSharpPlus.SlashCommands.Entities;

namespace CloudNine.Web.Commands
{
    public class GetQuoteCommand : BaseSlashCommandModule
    {
        public GetQuoteCommand(IServiceProvider p) : base(p) { }

        [SlashCommand("quote", 1)]
        public async Task GetQuoteCommandAsync(InteractionContext ctx)
        {

        }
    }
}
