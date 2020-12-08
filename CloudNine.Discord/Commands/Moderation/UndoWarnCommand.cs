using System.Threading.Tasks;

using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;

namespace CloudNine.Discord.Commands.Moderation
{
    public class UndoWarnCommand : BaseCommandModule
    {

        [Command("undo")]
        [Description("Undos an edit to a warn.")]
        [RequireUserPermissions(Permissions.ManageRoles)]
        public async Task UndoEditCommandAsync(CommandContext ctx,
            [Description("Warn to undo an edit for.")]
            string warnId)
        {

        }
    }
}
