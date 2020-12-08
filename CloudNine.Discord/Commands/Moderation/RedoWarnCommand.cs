using System.Threading.Tasks;

using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;

namespace CloudNine.Discord.Commands.Moderation
{
    public class RedoWarnCommand : BaseCommandModule
    {

        [Command("redo")]
        [Description("Redos an edit to a warn.")]
        [RequireUserPermissions(Permissions.ManageRoles)]
        public async Task RedoEditCommandAsync(CommandContext ctx,
            [Description("Warn to redo an edit for.")]
            string warnId)
        {

        }
    }
}
