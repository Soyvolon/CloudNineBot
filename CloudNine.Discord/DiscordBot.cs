using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

using CloudNine.Config.Bot;
using CloudNine.Discord.Commands.Birthday;
using CloudNine.Discord.Commands.Moderation;
using CloudNine.Discord.Interactions;
using CloudNine.Discord.Services;
using CloudNine.Discord.Utilities;

using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity;
using DSharpPlus.Interactivity.Extensions;
using DSharpPlus.SlashCommands;

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

        public const string VERSION = "2.0.0";
        private bool disposedValue;

        public static DiscordBot Bot { get; private set; }

        private readonly DiscordShardedClient _client;
        private readonly DiscordRestClient _rest;
        private readonly DiscordBotConfiguration _config;
        private readonly BirthdayManager _birthdays;
        private readonly IServiceProvider _services;

        public DiscordBot(IServiceProvider services,
            DiscordShardedClient client, DiscordRestClient rest, DiscordBotConfiguration config, BirthdayManager birthdays)
        {
            this._services = services;
            this._client = client;
            this._rest = rest;
            this._config = config;
            this._birthdays = birthdays;

            Bot = this;
        }

        private CommandsNextConfiguration GetCommandsNextConfiguration(DiscordBotConfiguration botCfg)
        {
            var cncfg = new CommandsNextConfiguration
            {
                CaseSensitive = false,
                EnableDms = false,
                IgnoreExtraArguments = true,
                Services = this._services,
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
                PaginationButtons = new()
                {
                    Left = new DiscordButtonComponent(ButtonStyle.Secondary, "left", "Back"),
                    SkipLeft = new DiscordButtonComponent(ButtonStyle.Secondary, "skipleft", "Start"),
                    Right = new DiscordButtonComponent(ButtonStyle.Secondary, "right", "Next"),
                    SkipRight = new DiscordButtonComponent(ButtonStyle.Secondary, "skipright", "End"),
                    Stop = new DiscordButtonComponent(ButtonStyle.Secondary, "end", "Close")
                }
            };

            return icfg;
        }

        public async Task StartAsync()
        {
            var commands = await _client.UseCommandsNextAsync(GetCommandsNextConfiguration(_config));
            foreach (var c in commands.Values)
            {
                c.RegisterCommands(Assembly.GetExecutingAssembly());

                c.CommandErrored += CommandResponder.RespondError;
                c.CommandExecuted += CommandResponder.RespondSuccess;

                c.RegisterConverter(new DateTimeAttributeConverter());

                c.SetHelpFormatter<CustomHelpFormatter>();

                var slash = c.Client.UseSlashCommands(new()
                {
                    Services = this._services
                });

                List<Type> parentGroups = new() { typeof(ModerationCommands), typeof(BirthdayCommands) };
                List<Type> ignoreGrouping = new();
                foreach (var p in parentGroups)
                    ignoreGrouping.AddRange(p.GetNestedTypes());

                var types = Assembly.GetAssembly(typeof(DiscordBot))?.GetTypes();
                if (types is not null)
                    foreach (var t in types)
                        if (t.IsSubclassOf(typeof(ApplicationCommandModule))
                            && !parentGroups.Any(x => t.IsSubclassOf(x))
                            && !ignoreGrouping.Contains(t))
#if DEBUG
                            slash.RegisterCommands(t, 750486424469372970);
#else
                            slash.RegisterCommands(t);
#endif
                slash.SlashCommandErrored += (x, y) =>
                {
                    Task.Run(async () =>
                    {
                        if (y.Exception is SlashExecutionChecksFailedException ex)
                        {
                            await y.Context.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource,
                                new DiscordInteractionResponseBuilder()
                                    .WithContent("You do not have permission to run that command."));
                        }
                        else
                        {
                            x.Client.Logger.LogError(y.Exception, $"Slash Command Errored.");
                        }
                    });
                    return Task.CompletedTask;
                };

                slash.ContextMenuErrored += (x, y) =>
                {
                    Task.Run(async () =>
                    {
                        if (y.Exception is SlashExecutionChecksFailedException ex)
                        {
                            await y.Context.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource,
                                new DiscordInteractionResponseBuilder()
                                    .WithContent("You do not have permission to run that command."));
                        }
                        else
                        {
                            x.Client.Logger.LogError(y.Exception, $"Context Menu Command Errored.");
                        }
                    });
                    return Task.CompletedTask;
                };
            }

            await _client.UseInteractivityAsync(GetInteractivityConfiguration());

            var commandHander = _services.GetRequiredService<CommandHandlerService>();

            _client.Ready += Client_Ready;

            var iservice = _services.GetRequiredService<MultisearchInteractivityService>();

            _client.MessageReactionAdded += iservice.Client_MessageReactionAdded;
            _client.MessageReactionRemoved += iservice.Client_MessageReactionRemoved;
            _client.MessageReactionsCleared += iservice.Client_MessageReactionsCleared;

            var ffrservice = _services.GetRequiredService<FanfictionLinkResponderService>();

            _client.MessageCreated += ffrservice.Client_MessageCreated;

            await _client.StartAsync();

            await Task.Run(async () =>
            {
                await Task.Delay(TimeSpan.FromSeconds(5));
                await _client.UpdateStatusAsync(
                    new DiscordActivity($"{_config.Prefix}help | {VERSION}", ActivityType.Playing));
            });
        }

        private Task Client_Ready(DiscordClient c, DSharpPlus.EventArgs.ReadyEventArgs e)
        {
            c?.Logger.LogInformation("Client Ready!");

            return Task.CompletedTask;
        }

        public async Task<DiscordClient?> GetClientForGuildId(ulong discordId)
        {
            foreach(var shard in _client.ShardClients.Values)
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
                    _client.StopAsync().GetAwaiter().GetResult();
                    foreach (var c in _client.ShardClients.Values)
                        c.Dispose();

                    _rest.Dispose();

                    _birthdays.Dispose();
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
