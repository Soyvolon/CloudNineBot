using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using CloudNine.Core.Configuration;
using CloudNine.Core.Database;
using CloudNine.Discord.Utilities;

using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace CloudNine.Discord.Services
{
    public class CommandHandlerService
    {
        private readonly ILogger _logger;
		private readonly ServiceProvider _services;
        private ConcurrentDictionary<MessageCreateEventArgs, Tuple<Task, CancellationTokenSource>> RunningCommands;

        public CommandHandlerService(ILogger<BaseDiscordClient> logger, ServiceProvider services)
        {
            this._logger = logger;
			this._services = services;

            RunningCommands = new ConcurrentDictionary<MessageCreateEventArgs, Tuple<Task, CancellationTokenSource>>();
        }

        public Task MessageRecievedAsync(DiscordClient source, MessageCreateEventArgs e)
        {
			if (e.Author.IsBot) return Task.CompletedTask;

			var cancelSource = new CancellationTokenSource();
			RunningCommands[e] = new Tuple<Task, CancellationTokenSource>(
				Task.Run(async () => await ExecuteCommand(source, e, cancelSource.Token)),
				cancelSource);

            return Task.CompletedTask;
        }

        private async Task ExecuteCommand(DiscordClient source, MessageCreateEventArgs e, CancellationToken cancellationToken)
        {
			try
            {
				cancellationToken.ThrowIfCancellationRequested();

				var model = _services.GetRequiredService<CloudNineDatabaseModel>();

				DiscordGuildConfiguration guildConfig = await model.FindAsync<DiscordGuildConfiguration>(e.Guild.Id);

				if (guildConfig is null)
				{
					guildConfig = new DiscordGuildConfiguration
					{
						Id = e.Guild.Id,
						Prefix = DiscordBot.Bot.BotConfiguration.Prefix
					};

					model.Add(guildConfig);

					await model.SaveChangesAsync();
				}

				cancellationToken.ThrowIfCancellationRequested();

				int prefixPos = await PrefixResolver(source, e.Message, guildConfig);

				if (prefixPos == -1)
					return; // Prefix is wrong, dont respond to this message.

				var prefix = e.Message.Content.Substring(0, prefixPos);
				string commandString = e.Message.Content.Substring(prefixPos);

				var cnext = source.GetCommandsNext();

				var command = cnext.FindCommand(commandString, out string args);

				cancellationToken.ThrowIfCancellationRequested();

				if (command is null)
				{ // Looks like that command does not exsist!
					await CommandResponder.RespondCommandNotFound(e.Channel, prefix);
				}
				else
				{ // We found a command, lets deal with it.
					var ctx = cnext.CreateContext(e.Message, prefix, command, args);
					// We are done here, its up to CommandsNext now.

					cancellationToken.ThrowIfCancellationRequested();

					await cnext.ExecuteCommandAsync(ctx);
				}
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Command Handler failed.");
			}
			finally
			{
				if (RunningCommands.TryRemove(e, out var taskData))
				{
					taskData.Item2.Dispose();
					taskData.Item1.Dispose();
				}
			}
		}

		private async Task<int> PrefixResolver(DiscordClient _client, DiscordMessage msg, DiscordGuildConfiguration guildConfig)
		{
			if (!msg.Channel.PermissionsFor(await msg.Channel.Guild.GetMemberAsync(_client.CurrentUser.Id).ConfigureAwait(false)).HasPermission(Permissions.SendMessages)) return -1; //Checks if bot can't send messages, if so ignore.
			else if (msg.Content.StartsWith(_client.CurrentUser.Mention)) return _client.CurrentUser.Mention.Length; // Always respond to a mention.
			else
			{
				try
				{
					if(guildConfig.Prefix is null)
                    {
						guildConfig.Prefix = DiscordBot.Bot.BotConfiguration.Prefix;
					}

					if (msg.Content.StartsWith(guildConfig.Prefix))
						return guildConfig?.Prefix?.Length ?? -1; //Return length of server prefix.
					else
						return -1;
				}
				catch (Exception err)
				{
					_logger.LogError(DiscordBot.Event_CommandHandler, $"Prefix Resolver failed in guild {msg.Channel.Guild.Name}:", DateTime.Now, err);
					return -1;
				}
			}
		}
	}
}
