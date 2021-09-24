using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using DSharpPlus.Interactivity;
using DSharpPlus.SlashCommands;

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CloudNine.Discord.Utilities
{
    public static class InteractivityExtensions
    {
        private static ConcurrentDictionary<ulong, (ImmutableArray<Page>, int, ulong)> ActiveInteractions { get; } = new();
        private static ConcurrentDictionary<ulong, Timer> Timeouts { get; } = new();

        private enum PaignateAction
        {
            First,
            Previous,
            Next,
            Last,
            Close,
            None
        }

        private static PaignateAction GetPaignateAction(this string value)
            => value switch
            {
                "first" => PaignateAction.First,
                "previous" => PaignateAction.Previous,
                "next" => PaignateAction.Next,
                "last" => PaignateAction.Last,
                "close" => PaignateAction.Close,
                _ => PaignateAction.None
            };

        private static DiscordComponent[] Components { get; } = new DiscordComponent[]
        {
            new DiscordButtonComponent(ButtonStyle.Primary, "first", "First", false),
            new DiscordButtonComponent(ButtonStyle.Primary, "previous", "Previous", false),
            new DiscordButtonComponent(ButtonStyle.Primary, "next", "Next", false),
            new DiscordButtonComponent(ButtonStyle.Primary, "last", "Last", false),
            new DiscordButtonComponent(ButtonStyle.Danger, "close", "Close", false)
        };

        public static async Task SendPaignatedMessageWithButtonsAsync(this InteractivityExtension interact, InteractionContext ctx, DiscordUser user, IEnumerable<Page> pages)
        {
            var builder = new DiscordFollowupMessageBuilder();
            var start = pages.FirstOrDefault();

            if (start is null) return;

            try
            {

                builder.AddEmbed(start.Embed)
                    .AddComponents(Components);

                var msg = await ctx.FollowUpAsync(builder);

                ActiveInteractions[msg.Id] = (pages.ToImmutableArray(), 0, user.Id);
                Timeouts[msg.Id] = new Timer(async (x) =>
                {
                    var updatedMessage = await msg.Channel.GetMessageAsync(msg.Id);
                    await CloseInteractionAsync(updatedMessage);
                }, null, TimeSpan.FromMinutes(2), Timeout.InfiniteTimeSpan);

                interact.Client.ComponentInteractionCreated += (s, e) =>
                {
                    _ = Task.Run(async () =>
                    {
                        if (ActiveInteractions.TryGetValue(e.Message.Id, out var data))
                        {
                            if (data.Item3 == e.User.Id)
                                await InteractionTriggered(e.Message, e.Interaction, data.Item1, data.Item2, data.Item3, GetPaignateAction(e.Id));
                        }
                    });

                    return Task.CompletedTask;
                };
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }

        private static async Task InteractionTriggered(DiscordMessage message, DiscordInteraction interaction, ImmutableArray<Page> pages, int index, ulong user, PaignateAction action)
        {
            switch (action)
            {
                case PaignateAction.First:
                    index = 0;
                    await UpdatePaignationAsync(interaction, message, pages[index]);
                    ActiveInteractions[message.Id] = (pages, index, user);
                    Timeouts[message.Id]?.Change(TimeSpan.FromMinutes(2), Timeout.InfiniteTimeSpan);
                    break;
                case PaignateAction.Previous:
                    index--;
                    if (index < 0)
                        index = pages.Length - 1;
                    await UpdatePaignationAsync(interaction, message, pages[index]);
                    ActiveInteractions[message.Id] = (pages, index, user);
                    Timeouts[message.Id]?.Change(TimeSpan.FromMinutes(2), Timeout.InfiniteTimeSpan);
                    break;
                case PaignateAction.Next:
                    index++;
                    if (index >= pages.Length)
                        index = 0;
                    await UpdatePaignationAsync(interaction, message, pages[index]);
                    ActiveInteractions[message.Id] = (pages, index, user);
                    Timeouts[message.Id]?.Change(TimeSpan.FromMinutes(2), Timeout.InfiniteTimeSpan);
                    break;
                case PaignateAction.Last:
                    index = pages.Length - 1;
                    await UpdatePaignationAsync(interaction, message, pages[index]);
                    ActiveInteractions[message.Id] = (pages, index, user);
                    Timeouts[message.Id]?.Change(TimeSpan.FromMinutes(2), Timeout.InfiniteTimeSpan);
                    break;
                case PaignateAction.Close:
                    await CloseInteractionAsync(interaction, message, pages[index]);
                    break;
                case PaignateAction.None:
                    break;
            }
        }

        private static async Task UpdatePaignationAsync(DiscordInteraction interaction, DiscordMessage message, Page page)
            => await interaction.CreateResponseAsync(InteractionResponseType.UpdateMessage, 
                new DiscordInteractionResponseBuilder()
                    .AddEmbed(page.Embed)
                    .AddComponents(Components));

        private static async Task CloseInteractionAsync(DiscordInteraction interaction, DiscordMessage message, Page page)
        {
            await interaction.CreateResponseAsync(InteractionResponseType.UpdateMessage,
                new DiscordInteractionResponseBuilder()
                    .AddEmbed(page.Embed));

            _ = ActiveInteractions.TryRemove(message.Id, out _);
            if (Timeouts.TryRemove(message.Id, out var timer))
            {
                await timer.DisposeAsync();
            }
        }

        private static async Task CloseInteractionAsync(DiscordMessage message)
        {
            var builder = new DiscordMessageBuilder
            {
                Embed = message.Embeds.FirstOrDefault(),
                Content = message.Content
            };

            await message.ModifyAsync(builder);

            _ = ActiveInteractions.TryRemove(message.Id, out _);
            if(Timeouts.TryRemove(message.Id, out var timer))
            {
                await timer.DisposeAsync();
            }
        }
    }
}
