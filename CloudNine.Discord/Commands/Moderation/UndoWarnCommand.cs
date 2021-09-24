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
        public partial class WarnCommands : SlashCommandBase
        {
            [SlashCommand("undo", "Undos an edit to a warn.")]
            [SlashRequireUserPermissions(Permissions.ManageMessages)]
            public async Task UndoEditCommandAsync(InteractionContext ctx,
                [Option("ID", "Warn to undo an edit for.")]
                string warnId)
            {
                var _database = _services.GetRequiredService<CloudNineDatabaseModel>();
                var mod = await _database.FindAsync<ModCore>(ctx.Guild.Id);

                if (mod is null)
                {
                    await ctx.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource);
                    await RespondError("There are no warnings on this server!");
                    return;
                }

                var warn = mod.WarnSet.FirstOrDefault(x => x.Key == warnId);

                if (warn is not null)
                {
                    if (warn.Undo())
                    {
                        _database.Update(mod);
                        await _database.SaveChangesAsync();

                        await ViewWarnCommand(ctx, warnId);
                    }
                    else
                    {
                        await ctx.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource);
                        await RespondError("Nothing to undo!");
                    }
                }
                else
                {
                    await ctx.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource);
                    await RespondError("No warn found.");
                }
            }
        }
    }
}
