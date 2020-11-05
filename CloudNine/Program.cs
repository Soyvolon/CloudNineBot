using System;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

using CloudNine.Config.Bot;
using CloudNine.Core.Database;
using CloudNine.Discord;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Scaffolding.Metadata;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using Newtonsoft.Json;

namespace CloudNine
{
    class Program
    {
        static void Main(string[] args)
        {
            Start().GetAwaiter().GetResult();
        }

        private static async Task Start()
        {
            ServiceCollection services = new ServiceCollection();
            services.AddLogging();

#if DEBUG
            LogLevel MinimumLogLevel = LogLevel.Debug;
#else
            LogLevel MinimumLogLevel = LogLevel.Error;
#endif

            services.AddLogging(o => o.SetMinimumLevel(MinimumLogLevel))
                .AddDbContext<CloudNineDatabaseModel>();

            await using var serviceProvider = services.BuildServiceProvider();

            await ApplyDatabaseMigrations(serviceProvider.GetRequiredService<CloudNineDatabaseModel>());

            using DiscordBot discord = new DiscordBot(MinimumLogLevel, serviceProvider);

            string json = "";
            using (FileStream fs = new FileStream("Config/bot_config.json", FileMode.Open))
            {
                using StreamReader sr = new StreamReader(fs);
                json = await sr.ReadToEndAsync();
            }

            var botCfg = JsonConvert.DeserializeObject<DiscordBotConfiguration>(json);

            await discord.Start(botCfg);

            await Task.Delay(-1);
        }

        private static async Task ApplyDatabaseMigrations(CloudNineDatabaseModel database)
        {
            if(!(await database.Database.GetPendingMigrationsAsync()).Any())
            {
                return;
            }

            await database.Database.MigrateAsync();
            await database.SaveChangesAsync();
        }
    }
}
