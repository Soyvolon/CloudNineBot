using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

using CloudNine.Config.Bot;
using CloudNine.Core.Database;
using CloudNine.Discord;
using CloudNine.Discord.Services;
using CloudNine.Services;

using DSharpPlus.Entities;

using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

using Newtonsoft.Json;

namespace CloudNine
{
    public class Program
    {
        public static DiscordBotConfiguration? DiscordConfig;
        public static DiscordBot? Discord;

        public static void Main(string[] args)
        {
            Start(args).GetAwaiter().GetResult();
        }

        private static async Task Start(string[] args)
        {
            ServiceCollection services = new ServiceCollection();
            services.AddLogging(o => o.AddConsole());

#if DEBUG
            LogLevel MinimumLogLevel = LogLevel.Debug;
#else
            LogLevel MinimumLogLevel = LogLevel.Error;
#endif

            services.AddLogging(o => o.SetMinimumLevel(MinimumLogLevel))
                .AddDbContext<CloudNineDatabaseModel>(ServiceLifetime.Transient, ServiceLifetime.Scoped)
                .AddSingleton<QuoteService>();

            await using var serviceProvider = services.BuildServiceProvider();

            await ApplyDatabaseMigrations(serviceProvider.GetRequiredService<CloudNineDatabaseModel>());

            Discord = new DiscordBot(MinimumLogLevel, serviceProvider);

            string json = "";
            using (FileStream fs = new FileStream("Config/bot_config.json", FileMode.Open))
            {
                using StreamReader sr = new StreamReader(fs);
                json = await sr.ReadToEndAsync();
            }

            DiscordConfig = JsonConvert.DeserializeObject<DiscordBotConfiguration>(json);

            try
            {
                await Discord.Start(DiscordConfig ?? new());
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            var host = Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                });

            host.Build().Run();

            await Task.Delay(-1);
        }

        private static async Task ApplyDatabaseMigrations(CloudNineDatabaseModel database)
        {
            if (!(await database.Database.GetPendingMigrationsAsync()).Any())
            {
                return;
            }

            await database.Database.MigrateAsync();
            await database.SaveChangesAsync();
        }
    }
}
