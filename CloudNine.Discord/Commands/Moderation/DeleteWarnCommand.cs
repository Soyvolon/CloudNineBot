using System;
using System.Linq;
using System.Threading.Tasks;

using CloudNine.Core.Database;
using CloudNine.Core.Moderation;

using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Interactivity.Extensions;
using DSharpPlus.SlashCommands;
using DSharpPlus.SlashCommands.Attributes;

using Microsoft.Extensions.DependencyInjection;

namespace CloudNine.Discord.Commands.Moderation
{
    public partial class ModerationCommands : SlashCommandBase
    {
        [SlashCommandGroup("warn", "Warn commands.")]
        public partial class WarnCommands : SlashCommandBase
        { 
            private readonly IServiceProvider _services;

            public WarnCommands(IServiceProvider services)
            {
                _services = services;
            }

            [SlashCommand("delete", "Premanetly deletes a warn")]
            [SlashRequireUserPermissions(Permissions.ManageGuild)]
            public async Task DeleteWarnCommandAsync(InteractionContext ctx,
                [Option("ID", "ID of the warn to delete.")]
                string warnId)
            {
                await ctx.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource);

                var _database = _services.GetRequiredService<CloudNineDatabaseModel>();
                var mod = await _database.FindAsync<ModCore>(ctx.Guild.Id);

                if (mod is null)
                {
                    await RespondError("There are no warnings on this server!");
                    return;
                }

                var warn = mod.WarnSet.FirstOrDefault(x => x.Key == warnId);

                if (warn is not null)
                {
                    mod.RemoveWarn(warnId);

                    _database.Update(mod);
                    await _database.SaveChangesAsync();

                    await RespondWarn($"Warn `{warnId}` was deleted succesfully.");
                }
                else
                {
                    await RespondError("No warn with that ID found.");
                }
            }
        }
    }
}
