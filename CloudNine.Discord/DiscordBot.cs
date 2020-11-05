using System;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;

using CloudNine.Config.Bot;
using CloudNine.Discord.Utilities;

using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.Entities;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using Newtonsoft.Json;

using static CloudNine.Discord.Utilities.AttributeConverters;

namespace CloudNine.Discord
{
    public class DiscordBot : IDisposable
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

        public const string VERSION = "alpha-2.0.0";
        private bool disposedValue;

        public static DiscordBot Bot { get; private set; }

        public DiscordClient Client { get; private set; }
        public DiscordRestClient Rest { get; private set; }
        public BirthdayManager Birthdays { get; private set; }

        private LogLevel logLevel;
        private ServiceProvider services;

        public DiscordBot(LogLevel logLevel, ServiceProvider services)
        {
            this.logLevel = logLevel;
            this.services = services;

            Bot = this;
        }

        private DiscordConfiguration GetDiscordConfiguration(DiscordBotConfiguration botCfg)
        {
            var cfg = new DiscordConfiguration
            {
                TokenType = TokenType.Bot,
                Token = botCfg.Token,
                MinimumLogLevel = logLevel
            };

            return cfg;
        }

        private CommandsNextConfiguration GetCommandsNextConfiguration(DiscordBotConfiguration botCfg)
        {
            var cncfg = new CommandsNextConfiguration
            {
                StringPrefixes = new string[] { botCfg.Prefix },
                CaseSensitive = false,
                EnableDms = false,
                IgnoreExtraArguments = true,
                Services = this.services
            };

            return cncfg;
        }

        public async Task Start(DiscordBotConfiguration botCfg)
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
                    new DiscordActivity($"{botCfg.Prefix}help | {VERSION}", ActivityType.Playing));
            });
        }

        private void InitalizeOtherParts(DiscordBotConfiguration cfg)
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

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects)
                    Client.DisconnectAsync().GetAwaiter().GetResult();
                    Client.Dispose();

                    Rest.Dispose();

                    Birthdays.Dispose();
                }

                // TODO: free unmanaged resources (unmanaged objects) and override finalizer
                // TODO: set large fields to null
                Bot = null;
                disposedValue = true;
            }
        }

        // // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
        // ~DiscordBot()
        // {
        //     // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        //     Dispose(disposing: false);
        // }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
