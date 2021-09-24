using System;
using System.Linq;
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
        [SlashCommandGroup("forgive", "Forgive command group.")]
        public partial class ForgiveCommands : SlashCommandBase
        {
            private readonly IServiceProvider _services;

            public ForgiveCommands(IServiceProvider services)
                => _services = services;

            [SlashCommand("warn", "Forgives a warn. Will note that this warn has been forgiven")]
            [SlashRequireUserPermissions(Permissions.ManageMessages)]
            public async Task ForgiveWarnCommandAsync(InteractionContext ctx,
                [Option("ID", "The warn ID to forgive (or not forgive).")]
                string warnId,
                [Option("Forgiveness", "Specify true to mark this warn as not forgiven.")]
                bool notForgiven = false)
            {
                await ctx.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource);

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
                    warn.NotForgiven = notForgiven;
                    warn.Forgiven = !notForgiven;

                    db.Update(mod);
                    await db.SaveChangesAsync();

                    await RespondWarn($"Warning was marked as {(notForgiven ? "Not Forgiven" : "Forgiven")}");
                }
            }

            [SlashCommand("clear", "Clears forgivness status on a warn.")]
            [SlashRequireUserPermissions(Permissions.ManageMessages)]
            public async Task ClearWarnStatusCommandAsync(InteractionContext ctx,
                [Option("ID", "The warn ID to forgive (or not forgive).")]
                string warnId)
            {
                await ctx.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource);

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
}
