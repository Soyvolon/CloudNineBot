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
    public class AddModlogNoticeCommand : CommandModule
    {
        private readonly IServiceProvider _services;

        public AddModlogNoticeCommand(IServiceProvider services)
        {
            this._services = services;
        }

        [Command("addmodlognotice")]
        [Description("Adds a notice when a users gains a specified number of warnings. Only one message is allowed per # of warns.")]
        [Aliases("amln", "addmodnotice")]
        [RequireUserPermissions(Permissions.ManageMessages)]
        public async Task AddModlogNoticeCommandAsync(CommandContext ctx,
            [Description("Amount of warns needed to trigger this notice.")]
            int warnCount,
            
            [Description("Message to send back to the moderator")]
            [RemainingText]
            string message)
        {
            var database = _services.GetRequiredService<CloudNineDatabaseModel>();
            var mod = await database.FindAsync<ModCore>(ctx.Guild.Id);

            if(mod is null)
            {
                mod = new ModCore(ctx.Guild.Id);
                await database.AddAsync(mod);
                await database.SaveChangesAsync();
            }

            mod.ModlogNotices[warnCount] = message;

            database.Update(mod);
            await database.SaveChangesAsync();

            await Respond($"Updated modlog notices for when a user receives {warnCount} warns and will now display the following message: ");
            await AddWarnCommand.DisplayWarnNotice(ctx, message, ctx.Member);
        }
    }
}
