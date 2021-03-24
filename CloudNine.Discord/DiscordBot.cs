using System;
using System.Reflection;
using System.Threading.Tasks;

using CloudNine.Config.Bot;
using CloudNine.Discord.Interactions;
using CloudNine.Discord.Services;
using CloudNine.Discord.Utilities;

using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity;
using DSharpPlus.Interactivity.Extensions;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using static CloudNine.Discord.Utilities.AttributeConverters;

namespace CloudNine.Discord
{
    public class DiscordBot : IDisposable
    {
        #region Event Ids
        // 127### - designates a Discord Bot event.
        public static EventId Event_CommandResponder { get; } = new EventId(127001, "Command Responder");
        public static EventId Event_CommandHandler { get; } = new EventId(127002, "Command Handler");
        public static EventId Event_EventHandler { get; } = new EventId(127003, "Event Handler");
        #endregion

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

        public const string VERSION = "1.5.2";
        private bool disposedValue;

        public static DiscordBot Bot { get; private set; }

        public DiscordShardedClient Client { get; private set; }
        public DiscordRestClient Rest { get; private set; }
        public BirthdayManager Birthdays { get; private set; }
        public DiscordBotConfiguration BotConfiguration { get; private set; }

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
                MinimumLogLevel = logLevel,
                Intents = DiscordIntents.DirectMessages | DiscordIntents.GuildMessageReactions 
                    | DiscordIntents.Guilds | DiscordIntents.GuildMessages
            };

            return cfg;
        }

        private CommandsNextConfiguration GetCommandsNextConfiguration(DiscordBotConfiguration botCfg)
        {
            var cncfg = new CommandsNextConfiguration
            {
                CaseSensitive = false,
                EnableDms = false,
                IgnoreExtraArguments = true,
                Services = this.services,
                UseDefaultCommandHandler = false,
                EnableDefaultHelp = true,
                
            };

            return cncfg;
        }

        private InteractivityConfiguration GetInteractivityConfiguration()
        {
            var icfg = new InteractivityConfiguration
            {
                PaginationBehaviour = DSharpPlus.Interactivity.Enums.PaginationBehaviour.WrapAround,
                PaginationDeletion = DSharpPlus.Interactivity.Enums.PaginationDeletion.DeleteEmojis,
            };

            return icfg;
        }

        public async Task Start(DiscordBotConfiguration botCfg)
        {
            BotConfiguration = botCfg;

            var cfg = GetDiscordConfiguration(botCfg);

            Client = new DiscordShardedClient(cfg);
            Rest = new DiscordRestClient(cfg);

            var commands = await Client.UseCommandsNextAsync(GetCommandsNextConfiguration(botCfg));
            foreach (var c in commands.Values)
            {
                c.RegisterCommands(Assembly.GetExecutingAssembly());

                c.CommandErrored += CommandResponder.RespondError;
                c.CommandExecuted += CommandResponder.RespondSuccess;

                c.RegisterConverter(new DateTimeAttributeConverter());

                c.SetHelpFormatter<CustomHelpFormatter>();
            }

            await Client.UseInteractivityAsync(GetInteractivityConfiguration());

            var commandHander = new CommandHandlerService(Client.Logger, services);

            Client.Ready += Client_Ready;
            Client.MessageCreated += commandHander.MessageRecievedAsync;

            var iservice = services.GetRequiredService<MultisearchInteractivityService>();

            Client.MessageReactionAdded += iservice.Client_MessageReactionAdded;
            Client.MessageReactionRemoved += iservice.Client_MessageReactionRemoved;
            Client.MessageReactionsCleared += iservice.Client_MessageReactionsCleared;

            var ffrservice = services.GetRequiredService<FanfictionLinkResponderService>();

            Client.MessageCreated += ffrservice.Client_MessageCreated;

            var relay = services.GetRequiredService<QuoteService>();
            Client.MessageCreated += relay.MessageRecievedAsync;

            InitalizeOtherParts(botCfg);

            await Client.StartAsync().ConfigureAwait(false);

            await Task.Run(async () =>
            {
                await Task.Delay(TimeSpan.FromSeconds(5));
                await Client.UpdateStatusAsync(
                    new DiscordActivity($"{botCfg.Prefix}help | {VERSION}", ActivityType.Playing));
            });
        }

        private void InitalizeOtherParts(DiscordBotConfiguration cfg)
        {
            Birthdays = new BirthdayManager(cfg.TriggerBday, services);
        }

        private Task Client_Ready(DiscordClient c, DSharpPlus.EventArgs.ReadyEventArgs e)
        {
            c?.Logger.LogInformation("Client Ready!");

            return Task.CompletedTask;
        }

        public async Task<DiscordClient?> GetClientForGuildId(ulong discordId)
        {
            foreach(var shard in Client.ShardClients.Values)
            {
                if (shard.Guilds.ContainsKey(discordId))
                    return shard;
            }

            return null;
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects)
                    Client.StopAsync().GetAwaiter().GetResult();
                    foreach (var c in Client.ShardClients.Values)
                        c.Dispose();

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
