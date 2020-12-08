using System.Linq;
using System.Threading.Tasks;

using CloudNine.Core.Database;
using CloudNine.Core.Moderation;

using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;

namespace CloudNine.Discord.Commands.Moderation
{
    public class RedoWarnCommand : CommandModule
    {
        private readonly CloudNineDatabaseModel _database;

        public RedoWarnCommand(CloudNineDatabaseModel database)
        {
            _database = database;
        }

        [Command("redo")]
        [Description("Redos an edit to a warn.")]
        [RequireUserPermissions(Permissions.ManageMessages)]
        public async Task RedoEditCommandAsync(CommandContext ctx,
            [Description("Warn to redo an edit for.")]
            string warnId)
        {
            var mod = await _database.FindAsync<ModCore>(ctx.Guild.Id);

            if (mod is null)
            {
                await RespondError("There are no warnings on this server!");
                return;
            }

            var warn = mod.WarnSet.FirstOrDefault(x => x.Key == warnId);

            if (warn is not null)
            {
                if (warn.Redo())
                {
                    _database.Update(mod);
                    await _database.SaveChangesAsync();

                    CommandsNextExtension? cnext = ctx.Client.GetCommandsNext();
                    string? raw = $"warn {warn.Key}";
                    Command? cmd = cnext.FindCommand(raw, out var args);

                    var context = cnext.CreateFakeContext(ctx.Member, ctx.Channel, raw, ctx.Prefix, cmd, args);

                    await cnext.ExecuteCommandAsync(context);
                }
                else
                {
                    await RespondError("Nothing to redo!");
                }
            }
            else
            {
                await RespondError("No warn found.");
            }
        }
    }
}
