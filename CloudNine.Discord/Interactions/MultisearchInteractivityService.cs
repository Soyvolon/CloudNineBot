using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using CloudNine.Core.Multisearch;
using CloudNine.Discord.Commands;

using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;

using Microsoft.Extensions.Logging;

namespace CloudNine.Discord.Interactions
{
    public class MultisearchInteractivityService
    {
        public const string LeftEmoji = ":arrow_left:";
        public const string RightEmoji = ":arrow_right:";
        public const string StopEmoji = ":stop_button:";

        private DiscordEmoji Left { get; set; }
        private DiscordEmoji Right { get; set; }
        private DiscordEmoji Stop { get; set; }
        private bool Initalized { get; set; }

        private readonly ILogger _logger;
        private readonly IServiceProvider _services;

        public ConcurrentDictionary<ulong, SearchManager> ActiveSearches { get; init; }
        public ConcurrentDictionary<ulong, MultisearchInteraction> Interactions { get; init; }

        public MultisearchInteractivityService(ILogger<MultisearchInteractivityService> logger, IServiceProvider services)
        {
            _logger = logger;
            _services = services;

            Interactions = new();
            ActiveSearches = new();
        }

        public void Initalize(DiscordClient c)
        {
            Left = DiscordEmoji.FromName(c, LeftEmoji);
            Right = DiscordEmoji.FromName(c, RightEmoji);
            Stop = DiscordEmoji.FromName(c, StopEmoji);
        }

        public async Task StartSearchDisplay(CommandContext ctx, SearchManager manager)
        {
            if (!Initalized)
                Initalize(ctx.Client);

            var embed = new DiscordEmbedBuilder();
            embed.WithTitle("Multisearch Results")
                .WithColor(CommandModule.Color_Search)
                .WithAuthor(ctx.Member.DisplayName, ctx.Member.AvatarUrl, ctx.Member.AvatarUrl)
                .WithTimestamp(DateTime.Now);

            var interact = new MultisearchInteraction(manager, ctx.Channel, ctx.User);
            var msg = await interact.StartAsync(embed);

            await msg.CreateReactionAsync(Left);
            await msg.CreateReactionAsync(Right);
            await msg.CreateReactionAsync(Stop);

            Interactions[msg.Id] = interact;
        }

        internal Task Client_MessageReactionsCleared(DiscordClient sender, MessageReactionsClearEventArgs e)
        {
            if (Interactions.TryRemove(e.Message.Id, out var i))
                _ = Task.Run(async () =>
                {
                    await i.StopAsync();
                });

            return Task.CompletedTask;
        }

        internal Task Client_MessageReactionRemoved(DiscordClient sender, MessageReactionRemoveEventArgs e)
        {
            if (e.User.IsBot) return Task.CompletedTask;

            if (!Initalized)
                Initalize(sender);

            if (Interactions.TryGetValue(e.Message.Id, out var i) && i.User.Id == e.User.Id)
                _ = Task.Run(async () =>
                {
                    await ParseEmote(e.Emoji, i);
                });

            return Task.CompletedTask;
        }

        internal Task Client_MessageReactionAdded(DiscordClient sender, MessageReactionAddEventArgs e)
        {
            if (e.User.IsBot) return Task.CompletedTask;

            if (!Initalized)
                Initalize(sender);

            if (Interactions.TryGetValue(e.Message.Id, out var i) && i.User.Id == e.User.Id)
                _ = Task.Run(async () =>
                {
                    await ParseEmote(e.Emoji, i);
                });

            return Task.CompletedTask;
        }

        private async Task ParseEmote(DiscordEmoji e, MultisearchInteraction i)
        {
            if (e.Name.Equals(Left.Name))
            {
                _logger.LogDebug("MultiSearch Interact - Running previous page method...");
                await i.PreviousPageAsync();
            }
            else if (e.Name.Equals(Right.Name))
            {
                _logger.LogDebug("MultiSearch Interact - Running next page method...");
                await i.NextPageAsync();
            }
            else if (e.Name.Equals(Stop.Name))
            {
                _logger.LogDebug("MultiSearch Interact - Running stop method...");
                await i.StopAsync();
            }
        }
    }
}
