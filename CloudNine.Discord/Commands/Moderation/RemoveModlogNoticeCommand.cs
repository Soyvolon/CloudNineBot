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
    public class RemoveModlogNoticeCommand : CommandModule
    {
        private readonly IServiceProvider _services;

        public RemoveModlogNoticeCommand(IServiceProvider services)
        {
            this._services = services;
        }

        [Command("rmeovemodlognotice")]
        [Description("Removes a modlog notice.")]
        [Aliases("rmmodnotice", "rmmn", "removemodnotice")]
        [RequireUserPermissions(Permissions.ManageMessages)]
        public async Task RemoveModlogNoticeCommandAsync(CommandContext ctx,
            [Description("Amount of warnings a notice is assigned to you intend to remove.")]
            int toRemove)
        {
            var database = _services.GetRequiredService<CloudNineDatabaseModel>();
            var mod = await database.FindAsync<ModCore>(ctx.Guild.Id);

            if(mod is null)
            {
                await RespondError("No mod configuration found! Add a wan or a notice to generate one!");
                return;
            }

            if(mod.ModlogNotices.TryGetValue(toRemove, out var notice))
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
