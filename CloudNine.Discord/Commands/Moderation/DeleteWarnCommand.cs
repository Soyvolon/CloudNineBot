using System;
using System.Linq;
using System.Threading.Tasks;

using CloudNine.Core.Database;
using CloudNine.Core.Moderation;

using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Interactivity.Extensions;

using Microsoft.Extensions.DependencyInjection;

namespace CloudNine.Discord.Commands.Moderation
{
    public class DeleteWarnCommand : CommandModule
    {
        private readonly IServiceProvider _services;

        public DeleteWarnCommand(IServiceProvider services)
        {
            _services = services;
        }

        [Command("deletewarn")]
        [Description("Premanetly deletes a warn")]
        [Aliases("delwarn")]
        [RequireUserPermissions(Permissions.ManageGuild)]
        public async Task DeleteWarnCommandAsync(CommandContext ctx,
            [Description("ID of the warn to delete.")]
            string warnId)
        {
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
                var interact = ctx.Client.GetInteractivity();

                await ctx.RespondAsync(":warning: **This action is irreversible!** :warning:\n" +
                    "**Re-enter the warn ID to confirm deletion of this warn and all of its edits.**");

                var res = await interact.WaitForMessageAsync(x => x.Author.Id == ctx.Message.Author.Id);

                if(res.TimedOut)
                {
                    await RespondError("Response timed out.");
                    return;
                }
                else if(res.Result.Content != warnId)
                {
                    await RespondError("Warn ID's do not match.");
                    return;
                }

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
