using System;
using System.Threading.Tasks;

using CloudNine.Core.Database;
using CloudNine.Core.Moderation;

using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;

using Microsoft.Extensions.DependencyInjection;

namespace CloudNine.Discord.Commands.Moderation
{
    public class SetReviewTimeSpanCommand : CommandModule
    {
        private readonly IServiceProvider _services;

        public SetReviewTimeSpanCommand(IServiceProvider services)
            => _services = services;

        [Command("reviewtime")]
        [Description("Sets the time before a warn is eligible for review. Defaults to 0.")]
        [Aliases("reviewspan")]
        [RequireUserPermissions(Permissions.ManageGuild)]
        public async Task SetReviewTimeSpanCommandAsync(CommandContext ctx, 
            [Description("Days until a warning is eligible for review. A value 0 or smaller will disable a time before" +
            " a warning is eligible.")]
            int days)
        {
            var db = _services.GetRequiredService<CloudNineDatabaseModel>();
            var mod = await db.FindAsync<ModCore>(ctx.Guild.Id);

            if(mod is null)
            {
                mod = new()
                {
                    GuildId = ctx.Guild.Id,
                };

                await db.AddAsync(mod);
                await db.SaveChangesAsync();
            }

            TimeSpan span;
            if (days <= 0)
                span = TimeSpan.Zero;
            else
                span = TimeSpan.FromDays(days);

            mod.ForgiveAfter = span;
            db.Update(mod);
            await db.SaveChangesAsync();

            await RespondWarn($"Updated time till a warn is eligible for forgiveness. Time is set to {mod.ForgiveAfter.TotalDays} days." +
                $" Use `{ctx.Prefix}toforgive` to see a list" +
                " of warnings eligible for forgiveness.");
        }
    }
}
