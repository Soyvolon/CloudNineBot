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
    public class ForgiveWarnCommand : CommandModule
    {
        private readonly IServiceProvider _services;

        public ForgiveWarnCommand(IServiceProvider services)
            => _services = services;

        [Command("forgivewarn")]
        [Description("Forgives a warn. This keeps the warn tracked but will note it in the system that is has been forgiven. A warn can be also marked as" +
            " not forgiven so it will not show up in review list.")]
        [Aliases("forgivew")]
        [RequireUserPermissions(Permissions.ManageMessages)]
        public async Task ForgiveWarnCommandAsync(CommandContext ctx, 
            [Description("The warn ID to forgive (or not forgive).")]
            string warnId,
            [Description("Specify `true` to mark this warn as not forgiven.")]
            bool notForgiven = false)
        {
            var db = _services.GetRequiredService<CloudNineDatabaseModel>();
            var mod = await db.FindAsync<ModCore>(ctx.Guild.Id);

            if (mod is null)
            {
                await RespondError("There are no warnings on this server!");
                return;
            }

            var warn = mod.WarnSet.FirstOrDefault(x => x.Key == warnId);

            if(warn is null)
            {
                await RespondError("No warning by that ID found.");
                return;
            }
            else
            {
                warn.NotForgiven = notForgiven;
                warn.Forgiven = !notForgiven;

                db.Update(mod);
                await db.SaveChangesAsync();

                await RespondWarn($"Warning was marked as {(notForgiven ? "Not Forgiven" : "Forgiven")}");
            }
        }

        [Command("clearwarnstatus")]
        [Description("Clears forgivness status on a warn.")]
        [RequireUserPermissions(Permissions.ManageMessages)]
        public async Task ClearWarnStatusCommandAsync(CommandContext ctx,
            [Description("The warn ID to forgive (or not forgive).")]
            string warnId)
        {
            var db = _services.GetRequiredService<CloudNineDatabaseModel>();
            var mod = await db.FindAsync<ModCore>(ctx.Guild.Id);

            if (mod is null)
            {
                await RespondError("There are no warnings on this server!");
                return;
            }

            var warn = mod.WarnSet.FirstOrDefault(x => x.Key == warnId);

            if (warn is null)
            {
                await RespondError("No warning by that ID found.");
                return;
            }
            else
            {
                warn.NotForgiven = false;
                warn.Forgiven = false;

                db.Update(mod);
                await db.SaveChangesAsync();

                await RespondWarn("Cleared forgiveness status for the warn.");
            }
        }
    }
}
