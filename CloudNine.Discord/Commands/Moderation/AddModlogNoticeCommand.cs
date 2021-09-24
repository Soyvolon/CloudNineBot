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
    [SlashCommandGroup("mod", "Moderation related commands.")]
    public partial class ModerationCommands : SlashCommandBase
    {
        [SlashCommandGroup("notice", "Moderation notice actions.")]
        public partial class NoticeActions : SlashCommandBase
        {
            private readonly IServiceProvider _services;

            public NoticeActions(IServiceProvider services)
            {
                this._services = services;
            }

            [SlashCommand("add", "Adds a notice when a users gains a specified number of warnings.")]
            [SlashRequireUserPermissions(Permissions.ManageMessages)]
            public async Task AddModlogNoticeCommandAsync(InteractionContext ctx,
                [Option("Warns", "Amount of warns needed to trigger this notice.")]
                long warnCountLong,

                [Option("Message", "Message to send back to the moderator")]
                string message)
            {
                int warnCount = (int)warnCountLong;

                var database = _services.GetRequiredService<CloudNineDatabaseModel>();
                var mod = await database.FindAsync<ModCore>(ctx.Guild.Id);

                if (mod is null)
                {
                    mod = new ModCore(ctx.Guild.Id);
                    await database.AddAsync(mod);
                    await database.SaveChangesAsync();
                }

                mod.ModlogNotices[warnCount] = message;

                database.Update(mod);
                await database.SaveChangesAsync();

                await Respond($"Updated modlog notices for when a user receives {warnCount} warns and will now display the following message: ");
                await WarnCommands.DisplayWarnNotice(ctx, message, ctx.Member);
            }
        }
    }
}
