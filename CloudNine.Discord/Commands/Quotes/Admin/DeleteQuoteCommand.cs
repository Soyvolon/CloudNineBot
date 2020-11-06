using System.Threading.Tasks;

using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;

namespace CloudNine.Discord.Commands.Quotes.Admin
{
    public class DeleteQuoteCommand : CommandModule
    {
        [Command("deletequote")]
        [Description("Deltes a quote by its ID.")]
        [Aliases("delquote", "dquote")]
        [RequireUserPermissions(Permissions.ManageMessages)]
        public async Task DeleteQuoteCommandAsync(CommandContext ctx)
        {

        }
    }
}
