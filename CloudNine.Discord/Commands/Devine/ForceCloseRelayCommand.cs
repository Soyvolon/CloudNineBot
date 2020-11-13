using System.Threading.Tasks;

using CloudNine.Discord.Services;
using CloudNine.Discord.Utilities;

using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;

namespace CloudNine.Discord.Commands.Devine
{
    public class ForceCloseRelayCommand : BaseCommandModule
    {
        private readonly QuoteRelayService _relay;

        public ForceCloseRelayCommand(QuoteRelayService relay)
        {
            this._relay = relay;
        }

        [Command("forcecloserelay")]
        [Description("Forces the active relay to close for your accoun.")]
        [Aliases("fcrelay")]
        [Hidden]
        public async Task ForceCloseRelayCommandAsync(CommandContext ctx)
        {
            if(await _relay.TryCloseRelayAsync(ctx.User))
            {
                await ctx.RespondAsync("Relay force closed succesffuly.");
            }
            else
            {
                await CommandResponder.RespondCommandNotFound(ctx.Channel, ctx.Prefix);
            }
        }
    }
}
