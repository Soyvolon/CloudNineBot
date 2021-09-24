using System;
using System.Threading.Tasks;

using CloudNine.Core.Database;
using CloudNine.Core.Moderation;

using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.SlashCommands;
using DSharpPlus.SlashCommands.Attributes;

using Microsoft.Extensions.DependencyInjection;

namespace CloudNine.Discord.Commands.Moderation
{
    public partial class ModerationCommands : SlashCommandBase
    {
        [SlashCommandGroup("review", "Review group.")]
        public partial class ReviewCommands : SlashCommandBase
        {
            private readonly IServiceProvider _services;

            public ReviewCommands(IServiceProvider services)
                => _services = services;

            [SlashCommand("time", "Sets the time before a warn is eligible for review. Defaults to 0.")]
            [SlashRequireUserPermissions(Permissions.ManageGuild)]
            public async Task SetReviewTimeSpanCommandAsync(InteractionContext ctx,
                [Option("Days", "Days until a warning is eligible for review. A value 0 or smaller will disable this.")]
                long days)
            {
                await ctx.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource);

                var db = _services.GetRequiredService<CloudNineDatabaseModel>();
                var mod = await db.FindAsync<ModCore>(ctx.Guild.Id);

                if (mod is null)
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
                    $" Use `/mod review list` to see a list" +
                    " of warnings eligible for forgiveness.");
            }
        }
    }
}
