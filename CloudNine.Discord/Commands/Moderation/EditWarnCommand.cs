using System.Threading.Tasks;

using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;

namespace CloudNine.Discord.Commands.Moderation
{
    public class EditWarnCommand : BaseCommandModule
    {

        [Command("editwarn")]
        [Description("Edits the reason behind a warn.")]
        [Aliases("edit")]
        [RequireUserPermissions(Permissions.ManageRoles)]
        public async Task EditWarnCommandAsync(CommandContext ctx,
            [Description("Warn to edit")]
            string warnId,

            [Description("New reason")]
            [RemainingText]
            string newReason)
        {

        }
    }
}
