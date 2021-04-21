using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

using CloudNine.Config.Bot;
using CloudNine.Core.Configuration;
using CloudNine.Core.Database;

using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;

using Microsoft.Extensions.DependencyInjection;

namespace CloudNine.Discord.Commands
{
    public class PrefixCommand : CommandModule
    {
        private readonly IServiceProvider _services;
        private readonly DiscordBotConfiguration _config;

        public PrefixCommand(IServiceProvider services, DiscordBotConfiguration config)
        {
            this._services = services;
            _config = config;
        }

        [Command("prefix")]
        [RequireGuild]
        [Description("Changes the prefix for your server.")]
        [RequireUserPermissions(Permissions.ManageGuild)]
        public async Task ChangePrefixCommandAsync(CommandContext ctx,
            [Description("New prefix to set. Leave blank to return to the default prefix.")]
            string? prefix = null)
        {
            var _database = _services.GetRequiredService<CloudNineDatabaseModel>();
            var cfg = await _database.FindAsync<DiscordGuildConfiguration>(ctx.Guild.Id);
            if(cfg is null)
            {
                cfg = new DiscordGuildConfiguration()
                {
                    Id = ctx.Guild.Id
                };

                await _database.AddAsync(cfg);
            }

            cfg.Prefix = prefix ?? _config.Prefix;

            await _database.SaveChangesAsync();

            await Respond($"Prefix set to: `{cfg.Prefix}`");
        }
    }
}
