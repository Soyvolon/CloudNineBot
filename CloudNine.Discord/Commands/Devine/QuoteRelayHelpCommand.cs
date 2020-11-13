using System.Threading.Tasks;

using CloudNine.Discord.Services;

using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;

namespace CloudNine.Discord.Commands.Devine
{
    public class QuoteRelayHelpCommand : BaseCommandModule
    {
        [Command("relayhelp")]
        [Description("Shows help information for the relay.")]
        [RequireUserPermissions(Permissions.ManageMessages)]
        [Hidden]
        public async Task RelayHelpCommandAsync(CommandContext ctx)
         => await ctx.RespondAsync(embed: QuoteRelayService.GetQuoteRelayHelp(ctx.Prefix));
    }
}
