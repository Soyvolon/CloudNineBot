using System;
using System.Linq;
using System.Threading.Tasks;

using CloudNine.Core.Database;
using CloudNine.Core.Moderation;

using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;

using Microsoft.Extensions.DependencyInjection;

namespace CloudNine.Discord.Commands.Moderation
{
    public class EditWarnCommand : CommandModule
    {
        private readonly IServiceProvider _services;

        public EditWarnCommand(IServiceProvider services)
        {
            _services = services;
        }

        [Command("editwarn")]
        [Description("Edits the reason behind a warn.")]
        [Aliases("edit")]
        [RequireUserPermissions(Permissions.ManageMessages)]
        public async Task EditWarnCommandAsync(CommandContext ctx,
            [Description("Warn to edit")]
            string warnId,

            [Description("New reason")]
            [RemainingText]
            string newReason)
        {
            var _database = _services.GetRequiredService<CloudNineDatabaseModel>();
            var mod = await _database.FindAsync<ModCore>(ctx.Guild.Id);

            if (mod is null)
            {
                await RespondError("There are no warnings on this server!");
                return;
            }

            var warn = mod.WarnSet.FirstOrDefault(x => x.Key == warnId);

            if(warn is not null)
            {
                warn.AddEdit(newReason == "" ? "Deafult Warn." : newReason);

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
                await RespondError("No warn found to edit.");
            }
        }
    }
}
