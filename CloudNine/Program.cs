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
using CloudNine.Discord.Utilities;

using DSharpPlus;
using DSharpPlus.SlashCommands;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using Newtonsoft.Json;

namespace CloudNine
{
    class Program
    {
        static void Main(string[] args)
        {
            Start(args).GetAwaiter().GetResult();
        }

        private static async Task Start(string[] args)
        {
            ServiceCollection services = new ServiceCollection();

#if DEBUG
            LogLevel MinimumLogLevel = LogLevel.Debug;
#else
            LogLevel MinimumLogLevel = LogLevel.Error;
#endif

            services.AddLogging(o => o.AddConsole().SetMinimumLevel(MinimumLogLevel))
                .AddDbContext<CloudNineDatabaseModel>(ServiceLifetime.Transient, ServiceLifetime.Scoped)
                .AddSingleton<QuoteService>()
                .AddSingleton<HttpClient>()
                .AddTransient<SearchParseSevice>()
                .AddSingleton<MultisearchInteractivityService>()
                .AddSingleton<FanfictionLinkResponderService>()
                .AddSingleton<MagicChannelService>();

            services.AddHttpClient<FanfictionClient>(client =>
            {
                // do cookie setup here.
                client.DefaultRequestHeaders.Add("Cookie", "view_adult=true");
            });

            string json = "";
            using (FileStream fs = new FileStream("Config/bot_config.json", FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                using StreamReader sr = new StreamReader(fs);
                json = await sr.ReadToEndAsync();
            }

            var discordConfig = JsonConvert.DeserializeObject<DiscordBotConfiguration>(json);

            services.AddSingleton(discordConfig)
                .AddSingleton(GetDiscordConfiguration(discordConfig, MinimumLogLevel))
                .AddSingleton<DiscordShardedClient>()
                .AddSingleton<DiscordRestClient>()
                .AddSingleton<BirthdayManager>((x) => new(discordConfig.TriggerBday, x))
                .AddSingleton<CommandHandlerService>()
                .AddSingleton<DiscordBot>();

            await using var serviceProvider = services.BuildServiceProvider();

            await ApplyDatabaseMigrations(serviceProvider.GetRequiredService<CloudNineDatabaseModel>());

            try
            {
                await serviceProvider.GetRequiredService<DiscordBot>().StartAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            var chans = serviceProvider.GetRequiredService<MagicChannelService>();

            await chans.InitalizeAsync();

            await Task.Delay(-1);
        }

        private static async Task ApplyDatabaseMigrations(DbContext database)
        {
            if (!(await database.Database.GetPendingMigrationsAsync()).Any())
            {
                return;
            }

            await database.Database.MigrateAsync();
            await database.SaveChangesAsync();
        }

        private static DiscordConfiguration GetDiscordConfiguration(DiscordBotConfiguration botCfg, LogLevel minLogLevel)
        {
            var cfg = new DiscordConfiguration
            {
                TokenType = TokenType.Bot,
                Token = botCfg.Token,
                MinimumLogLevel = minLogLevel,
                Intents = DiscordIntents.DirectMessages | DiscordIntents.GuildMessageReactions
                    | DiscordIntents.Guilds | DiscordIntents.GuildMessages | DiscordIntents.GuildMembers
            };

            return cfg;
        }
    }
}
