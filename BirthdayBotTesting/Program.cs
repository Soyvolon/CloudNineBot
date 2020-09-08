using System;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;

using DSharpPlus;
using DSharpPlus.CommandsNext;

using Microsoft.Extensions.Logging;

using Newtonsoft.Json;

using static BirthdayBotTesting.AttributeConverters;

namespace BirthdayBotTesting
{
    public struct BotConfig
    {
        [JsonProperty("token")]
        public string Token { get; private set; }

        [JsonProperty("prefix")]
        public string Prefix { get; private set; }
        [JsonProperty("trigger_bday_time")]
        public int TriggerBday { get; private set; }
    }

    public class Program
    {
        public Program() { }

        public static bool IsDebug
        {
            get
            {
#if DEBUG
                return true;
#else
                return false;
#endif
            }
        }

        public static Program Bot { get; private set; }

        public DiscordShardedClient Client { get; private set; }
        public DiscordRestClient Rest { get; private set; }
        public BirthdayManager Birthdays { get; private set; }

        static void Main(string[] args)
        {
            try
            {
                var botCfgPath = Path.Combine(new string[] { "Config", "bot_config.json" });

                botCfgPath = Path.GetFullPath(botCfgPath);

                if (!Directory.Exists("Config"))
                    Directory.CreateDirectory("Config");

                if (!File.Exists(botCfgPath))
                {
                    File.WriteAllText(botCfgPath, @"{ ""token"": ""insert_token_here"", ""prefix"": ""bb"" }");
                    Console.WriteLine("Bot config was missing, please insert new token.");
                    Console.ReadLine();
                    Environment.Exit(-1);
                }

                using FileStream fs = new FileStream(botCfgPath, FileMode.Open);
                using StreamReader sr = new StreamReader(fs);
                var json = sr.ReadToEnd();

                var botCfg = JsonConvert.DeserializeObject<BotConfig>(json);

                Bot = new Program();

                Bot.Start(botCfg).GetAwaiter().GetResult();

                while (!Console.ReadLine().Equals("exit")) { }

                Bot.Birthdays.SaveAllConfigurations();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Console.WriteLine(ex.StackTrace);
                Console.WriteLine(ex.TargetSite);
                foreach (var pair in ex.Data)
                    Console.WriteLine(pair?.ToString());
                Console.WriteLine();
                Console.WriteLine(ex.InnerException?.Message);
                Console.WriteLine(ex.InnerException?.StackTrace);
            }
        }

        private static DiscordConfiguration GetDiscordConfiguration(BotConfig botCfg)
        {
            var cfg = new DiscordConfiguration
            {
                TokenType = TokenType.Bot,
                Token = botCfg.Token,
                MinimumLogLevel = LogLevel.Information
            };

            return cfg;
        }

        private static CommandsNextConfiguration GetCommandsNextConfiguration(BotConfig botCfg)
        {
            var cncfg = new CommandsNextConfiguration
            {
                StringPrefixes = new string[] { botCfg.Prefix },
                CaseSensitive = false,
                EnableDms = false,
                IgnoreExtraArguments = true,
            };

            return cncfg;
        }

        public async Task Start(BotConfig botCfg)
        {
            try
            {
                var cfg = GetDiscordConfiguration(botCfg);

                Client = new DiscordShardedClient(cfg);
                Rest = new DiscordRestClient(cfg);

                var commands = await Client.UseCommandsNextAsync(GetCommandsNextConfiguration(botCfg)).ConfigureAwait(false);

                foreach (var client in commands.Values)
                {
                    client.RegisterCommands(Assembly.GetExecutingAssembly());

                    client.CommandErrored += Client_CommandErrored;
                    client.CommandExecuted += Client_CommandExecuted;

                    client.RegisterConverter(new DateTimeAttributeConverter());
                }

                Client.Ready += Client_Ready;

                InitalizeOtherParts(botCfg);

                await Client.StartAsync().ConfigureAwait(false);

                Console.WriteLine("Starting");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Console.WriteLine(ex.StackTrace);
                Console.WriteLine(ex.TargetSite);
                foreach (var pair in ex.Data)
                    Console.WriteLine(pair?.ToString());
                Console.WriteLine();
                Console.WriteLine(ex.InnerException?.Message);
                Console.WriteLine(ex.InnerException?.StackTrace);
            }
        }

        private void InitalizeOtherParts(BotConfig cfg)
        {
            Birthdays = new BirthdayManager(cfg.TriggerBday);
        }

        public DiscordClient GetShardForGuild(ulong guildId)
        {
            foreach (var shard in Client.ShardClients.Values)
            {
                if (shard.Guilds.ContainsKey(guildId))
                    return shard;
            }

            return null;
        }

        private Task Client_Ready(DSharpPlus.EventArgs.ReadyEventArgs e)
        {
            Console.WriteLine("Entering Client_Ready");
            e.Client?.Logger.LogInformation("Client Ready!");

            return Task.CompletedTask;
        }

        private Task Client_CommandExecuted(CommandExecutionEventArgs e)
        {
            e.Context.Client.Logger.LogInformation($"User {e.Context.User.Username} executed command: {e.Command.Name}");

            return Task.CompletedTask;
        }

        private Task Client_CommandErrored(CommandErrorEventArgs e)
        {
            e.Context.Client.Logger.LogError(e.Exception, $"User {e.Context.User.Username} failed to execute command: {e.Command?.Name}");

            Task.Run(async () =>
            {
                await e.Context.RespondAsync($"Command Errored:\n{e.Exception.Message}").ConfigureAwait(false);
            });

            return Task.CompletedTask;
        }
    }
}
