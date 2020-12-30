using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

using DSharpPlus.CommandsNext;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;

namespace CloudNine.Atmo.State
{
    public abstract class StateMachine : BaseState
    {
        public CommandContext Context { get; internal set; }
        public abstract Task<bool> Execute(ulong userId, params string[] args);

        public StateMachine(CommandContext ctx)
        {
            Context = ctx;
        }

        protected async Task<DiscordMessage> RespondAsync(string message = "", DiscordEmbed? embed = null)
        {
            return await Context.RespondAsync(message, embed: embed);
        }
    }
}
