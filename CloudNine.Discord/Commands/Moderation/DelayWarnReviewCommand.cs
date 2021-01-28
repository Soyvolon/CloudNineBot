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
    public class DelayWarnReviewCommand : CommandModule
    {
        private readonly IServiceProvider _services;

        public DelayWarnReviewCommand(IServiceProvider services)
            => _services = services;

        [Command("delaywarnreview")]
        [Description("Delays the review of a warn for the specified amount of days.")]
        [Aliases("delayreview")]
        [RequireUserPermissions(Permissions.ManageMessages)]
        public async Task DelayWarnReviewCommandAsync(CommandContext ctx,
            [Description("The warn ID to forgive (or not forgive).")]
            string warnId,
            [Description("Days to delay review by")]
            int days)
        {
            if(days <= 0)
            {
                await RespondError("Days can not be less than or equal to 0.");
                return;
            }

            var db = _services.GetRequiredService<CloudNineDatabaseModel>();
            var mod = await db.FindAsync<ModCore>(ctx.Guild.Id);

            if (mod is null)
                await RespondError("There are no warnings on this server.");
            else
            {
                var warn = mod.WarnSet.FirstOrDefault(x => x.Key == warnId);

                if(warn is null)
                {
                    await RespondError("No warn by that ID was found.");
                    return;
                }

                var toReviewOn = DateTime.UtcNow.AddDays(days);

                warn.IgnoreUntil = toReviewOn;

                db.Update(mod);
                await db.SaveChangesAsync();

                await RespondWarn($"Review for `{warn.Key}` until {toReviewOn:D}");
            }
        }
    }
}
