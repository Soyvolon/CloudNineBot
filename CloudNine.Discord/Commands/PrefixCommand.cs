using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

using CloudNine.Core.Configuration;
using CloudNine.Core.Database;

using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;

namespace CloudNine.Discord.Commands
{
    public class PrefixCommand : CommandModule
    {
        private readonly CloudNineDatabaseModel _database;

        public PrefixCommand(CloudNineDatabaseModel database)
        {
            this._database = database;
        }

        [Command("prefix")]
        [Description("Changes the prefix for your server.")]
        [RequireUserPermissions(Permissions.ManageGuild)]
        public async Task ChangePrefixCommandAsync(CommandContext ctx,
            [Description("New prefix to set. Leave blank to return to the default prefix.")]
            string? prefix = null)
        {
            var cfg = await _database.FindAsync<DiscordGuildConfiguration>(ctx.Guild.Id);
            if(cfg is null)
            {
                cfg = new DiscordGuildConfiguration()
                {
                    Id = ctx.Guild.Id
                };

                await _database.AddAsync(cfg);
            }

            cfg.Prefix = prefix ?? DiscordBot.Bot.BotConfiguration.Prefix;

            await _database.SaveChangesAsync();

            await Respond($"Prefix set to: `{cfg.Prefix}`");
        }
    }
}
