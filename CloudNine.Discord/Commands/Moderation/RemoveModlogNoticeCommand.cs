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
        public partial class NoticeActions : SlashCommandBase
        {
            [SlashCommand("remove", "Removes a modlog notice.")]
            [SlashRequireUserPermissions(Permissions.ManageMessages)]
            public async Task RemoveModlogNoticeCommandAsync(InteractionContext ctx,
                [Option("Count", "Amount of warnings a notice is assigned to you intend to remove.")]
                long toRemoveLong)
            {
                await ctx.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource);

                int toRemove = (int)toRemoveLong;

                var database = _services.GetRequiredService<CloudNineDatabaseModel>();
                var mod = await database.FindAsync<ModCore>(ctx.Guild.Id);

                if (mod is null)
                {
                    await RespondError("No mod configuration found! Add a wan or a notice to generate one!");
                    return;
                }

                if (mod.ModlogNotices.TryGetValue(toRemove, out var notice))
                {
                    database.Update(mod);
                    await database.SaveChangesAsync();

                    await Respond($"Removed notice for {toRemove} warnings: {notice}");
                }
                else
                {
                    await RespondError($"Failed to find a notice for {toRemove} warnings.");
                }
            }
        }
    }
}
