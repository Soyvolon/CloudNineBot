using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

using CloudNine.Config.Bot;
using CloudNine.Core.Database;
using CloudNine.Core.Http;
using CloudNine.Core.Multisearch;
using CloudNine.Core.Multisearch.Searching;
using CloudNine.Discord;
using CloudNine.Discord.Interactions;
using CloudNine.Discord.Services;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using Newtonsoft.Json;

namespace CloudNine
{
    class Program
    {
        public static DiscordBotConfiguration? DiscordConfig;
        public static DiscordBot? Discord;

        static void Main(string[] args)
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
                .AddSingleton<QuoteService>()
                .AddSingleton<HttpClient>()
                .AddTransient<SearchParseSevice>()
                .AddSingleton<MultisearchInteractivityService>()
                .AddSingleton<FanfictionLinkResponderService>();

            services.AddHttpClient<FanfictionClient>(client =>
            {
                // do cookie setup here.
                client.DefaultRequestHeaders.Add("Cookie", "view_adult=true");
            });

            await using var serviceProvider = services.BuildServiceProvider();

            await ApplyDatabaseMigrations(serviceProvider.GetRequiredService<CloudNineDatabaseModel>());

            Discord = new DiscordBot(MinimumLogLevel, serviceProvider);

            string json = "";
            using (FileStream fs = new FileStream("Config/bot_config.json", FileMode.Open, FileAccess.Read, FileShare.Read))
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
