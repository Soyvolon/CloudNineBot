using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using CloudNine.Core.Configuration;
using CloudNine.Core.Database;
using CloudNine.Core.Multisearch;

using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Interactivity.Extensions;

using Microsoft.Extensions.DependencyInjection;

namespace CloudNine.Discord.Commands.Utility
{
    public class PurgeCommand : CommandModule
    {
        private readonly IServiceProvider _services;
        private readonly List<string> positiveResponses = new List<string>() { "yes", "y" };

        public PurgeCommand(IServiceProvider services)
        {
            _services = services;
        }

        [Command("userpurge")]
        [Description("Purges all user data from the bot. As long as a command that uses user data is not run, the user data will not be generated again.")]
        [Aliases("upurge")]
        public async Task PurgeUserDataCommandAsync(CommandContext ctx)
        {
            var interact = ctx.Client.GetInteractivity();
            await RespondWarn("You are about to remove all user data from Cloud Nine Bot." +
                " All Multisearch configuration, cache, and other saved data will be removed." +
                " Are you sure you wish to continue? `(Y)es/(N)o`");

            var confirm = await interact.WaitForMessageAsync(x => x.Author.Id == ctx.User.Id);

            if (confirm.TimedOut)
                await RespondError("Request timed out.");
            else if (!positiveResponses.Contains(confirm.Result.Content.ToLower()))
                await RespondError("Canceling purge.");
            else
            {

                var db = _services.GetRequiredService<CloudNineDatabaseModel>();
                var user = await db.FindAsync<MultisearchUser>(ctx.Member.Id);
                db.Remove(user);
                await db.SaveChangesAsync();

                await RespondWarn("Purge complete.");
            }
        }

        [Command("guildpurge")]
        [Description("Purges all server data from the bot. As long as a command that does not save data is run, the server data will not be generated again.")]
        [Aliases("gpurge")]
        public async Task PurgeGuildDataCommandAsync(CommandContext ctx)
        {
            var interact = ctx.Client.GetInteractivity();
            await RespondWarn("You are about to remove all user data from Cloud Nine Bot." +
                " All Multisearch configuration, cache, and other saved data will be removed." +
                " All quotes will be deleted. All warning data will be deleted. A cusom prefix will be reset." +
                " Are you sure you wish to continue? `(Y)es/(N)o`");

            var confirm = await interact.WaitForMessageAsync(x => x.Author.Id == ctx.User.Id);

            if (confirm.TimedOut)
                await RespondError("Request timed out.");
            else if (!positiveResponses.Contains(confirm.Result.Content.ToLower()))
                await RespondError("Canceling purge.");
            else
            {
                var db = _services.GetRequiredService<CloudNineDatabaseModel>();
                var guild = await db.FindAsync<DiscordGuildConfiguration>(ctx.Guild.Id);
                db.Remove(guild);
                await db.SaveChangesAsync();

                await RespondWarn("Purge complete.");
            }
        }
    }
}
