using System;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;

using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.Entities;

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

        public const string VERSION = "1.0.1";

        public static Program Bot { get; private set; }

        public DiscordClient Client { get; private set; }
        public DiscordRestClient Rest { get; private set; }
        public BirthdayManager Birthdays { get; private set; }

        static void Main(string[] args)
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

            //while (!Console.ReadLine().Equals("exit")) { }

            Task.Delay(-1).GetAwaiter().GetResult();

            Bot.Birthdays.SaveAllConfigurations();
        }

        private static DiscordConfiguration GetDiscordConfiguration(BotConfig botCfg)
        {
            var cfg = new DiscordConfiguration
            {
                TokenType = TokenType.Bot,
                Token = botCfg.Token,
                MinimumLogLevel = LogLevel.Debug
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
            var cfg = GetDiscordConfiguration(botCfg);

            Client = new DiscordClient(cfg);
            Rest = new DiscordRestClient(cfg);

            var commands = Client.UseCommandsNext(GetCommandsNextConfiguration(botCfg));

            commands.RegisterCommands(Assembly.GetExecutingAssembly());

            commands.CommandErrored += Client_CommandErrored;
            commands.CommandExecuted += Client_CommandExecuted;

            commands.RegisterConverter(new DateTimeAttributeConverter());

            Console.WriteLine(Client.GetCommandsNext());

            Client.Ready += Client_Ready;

            InitalizeOtherParts(botCfg);

            await Client.ConnectAsync().ConfigureAwait(false);

            Console.WriteLine("Starting");

            await Task.Run(async () =>
            {
                await Task.Delay(TimeSpan.FromSeconds(5));
                await Client.UpdateStatusAsync(
                    new DiscordActivity($"bbhelp | BBB {VERSION}", ActivityType.Playing));
            });
        }

        private void InitalizeOtherParts(BotConfig cfg)
        {
            Birthdays = new BirthdayManager(cfg.TriggerBday);
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
