using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

using CloudNine.Core.Quotes;
using CloudNine.Discord.Commands;

using ConcurrentCollections;

using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;

using Microsoft.Extensions.Logging;

namespace CloudNine.Discord.Services
{
    public class QuoteRelayService
    {
        public enum LatchMode
        {
            Last,
            Default
        }

        public class Relay
        {
            public DiscordGuild Destination { get; set; }
            public ulong SourceId { get; set; }
            public char ActionKey { get; set; }

            public ConcurrentDictionary<string, LatchMode> Latches { get; } = new ConcurrentDictionary<string, LatchMode>();
            public RelayData Data { get; } = new RelayData();
        }

        public class RelayData
        {
            // Latch Variables
            public ulong? ChannelId { get; set; }
            public string? Author { get; set; }
            public string? SavedBy { get; set; }
            public string? ImageUrl { get; set; }
            public DiscordColor? Color { get; set; }
        }

        public ConcurrentDictionary<DiscordUser, Relay> ActiveLinks { get; init; }

        private readonly ILogger _logger;
        private ConcurrentDictionary<MessageCreateEventArgs, Tuple<Task, CancellationTokenSource>> RunningRelays;

        public QuoteRelayService(ILogger<QuoteRelayService> logger)
        {
            this._logger = logger;

            ActiveLinks = new ConcurrentDictionary<DiscordUser, Relay>();
            RunningRelays = new ConcurrentDictionary<MessageCreateEventArgs, Tuple<Task, CancellationTokenSource>>();
        }

        public List<string> GetParamsString(string input)
        {
            return Regex.Matches(input, @"(['\""])(?<value>.+?)\1|(?<value>[^ ]+)")
                .Cast<Match>()
                .Select(m => m.Groups["value"].Value)
                .ToList();
        }

        public async Task<bool> TryOpenRelayAsync(DiscordUser source, DiscordChannel sourceChannel, ulong destGuildId, char actionKey)
        {
            if (ActiveLinks.ContainsKey(source)) return false;

            var client = await DiscordBot.Bot.GetClientForGuildId(destGuildId);

            if (client is null) return false;

            var guild = await client.GetGuildAsync(destGuildId);

            ActiveLinks[source] = new Relay
            {
                Destination = guild,
                SourceId = sourceChannel.Id,
                ActionKey = actionKey
            };

            return true;
        }

        public Task MessageRecievedAsync(DiscordClient source, MessageCreateEventArgs e)
        {
            var cancelSource = new CancellationTokenSource();
            RunningRelays[e] = new Tuple<Task, CancellationTokenSource>(
                Task.Run(async () => await Relay_MessageCreatedAsync(source, e)),
                cancelSource);

            return Task.CompletedTask;
        }

        private async Task Relay_MessageCreatedAsync(DiscordClient source, MessageCreateEventArgs e)
        {
            try
            {
                if (ActiveLinks.TryGetValue(e.Author, out var r))
                {
                    var args = GetParamsString(e.Message.Content);

                    if (args.Count > 0 && args[0].StartsWith(r.ActionKey))
                    {
                        if (args[0].Equals(r.ActionKey))
                            args.RemoveAt(0);
                        else if (args[0].Length > 1)
                            args[0] = args[0][1..];
                        else
                            return; // failed to separate the prefix. This should be impossible.

                        await SendQuoteRelay(e.Message, r, args);
                    }
                }
            }
            finally
            {
                if (RunningRelays.TryRemove(e, out var taskData))
                {
                    taskData.Item2.Dispose();
                    taskData.Item1.Dispose();
                }
            }
        }

        private async Task SendQuoteRelay(DiscordMessage source, Relay relay, List<string> args)
        {
            var quote = new Quote();
            var data = new RelayData();

            List<string> content = new List<string>();
            for(int i = 0; i < args.Count; i++)
            {
                switch(args[i])
                {

                    #region RELAY Commands
                    case "-c":
                    case "--channel":

                        break;

                    case "-a":
                    case "--author":
                        if (args.Count <= i + 1)
                        {
                            await source.Channel.SendMessageAsync("Failed to parse `--author`, not enough paramaters.");
                            return;
                        }
                        else
                            quote.Author = args[++i];
                        break;

                    case "-s":
                    case "--saved":
                        if (args.Count <= i + 1)
                        {
                            await source.Channel.SendMessageAsync("Failed to parse `--saved`, not enough paramaters.");
                            return;
                        }
                        else
                            quote.SavedBy = args[++i];
                        break;

                    case "-i":
                    case "--image":
                        string url = "";
                        if (args.Count <= i + 1)
                        {
                            if (source.Attachments.Count > 0)
                            {
                                url = source.Attachments[0].Url;
                            }
                            else
                            {
                                await source.Channel.SendMessageAsync("Failed to parse `--image`, not enough paramaters, and/or image not attached.");
                                return;
                            }
                        }
                        else
                        {
                            try
                            {
                                var uri = new Uri(args[++i]);
                                url = uri.AbsoluteUri;
                            }
                            catch { } // just leave url as "", it will be skipped later.

                        }

                        if (url != "")
                        {
                            quote.Attachment = url;
                        }
                        else
                        {
                            await source.Channel.SendMessageAsync("Failed to get a valid URL.");
                            return;
                        }
                        break;

                    case "-C":
                    case "--color":

                        break;

                    #endregion

                    #region NON-RELAY Commands
                    case "-h":
                    case "--help":
                        await source.Channel.SendMessageAsync(embed: GetQuoteRelayHelp(""));
                        return;

                    case "-L":
                    case "--latch":

                        return;

                    case "--shutdown":
                        await TryCloseRelayAsync(source.Author);
                        await source.Channel.SendMessageAsync(embed: new DiscordEmbedBuilder()
                            .WithColor(DiscordColor.DarkRed)
                            .WithDescription("Relay Closed."));
                        return;
                    #endregion

                    default:
                        content.Add(args[i]);
                        break;
                }
            }
        }

        public Task<bool> TryCloseRelayAsync(DiscordUser source)
        {
            return Task.FromResult(ActiveLinks.TryRemove(source, out _));
        }

        public static DiscordEmbedBuilder GetQuoteRelayHelp(string prefix)
        {
            return new DiscordEmbedBuilder()
                    .WithColor(CommandModule.Color_Cloud)
                    .WithTitle("Quote Relay Help")
                    .WithDescription("Detailed help for the Quote Relay.\n" +
                        $"Using `{prefix}relayhelp` or `--help` as a relay action will get you this help command.")
                    .AddField($"INFO", $"```http\n" +
                        $"General Information :: The body of your message does not need to be wrapped in quotes (\"), " +
                        $"but the body of any command does if it is more than a single word long.\n" +
                        $"Send Message        :: Send a default message by using your Action Key before any plain text. The message" +
                        $" must start with your action prefix or it will be ingored, and the text will become the Content of the quote.\n" +
                        $"Command Parsing     :: The entire command needs to parse succesfully before it will be sent. Sent messages will be given a reaction, while errors" +
                        $" will be set back.\n" +
                        $"\n```")
                    .AddField("RELAY Commands", 
                        "Use any of the following modifiers after the full Content of your quote." +
                        " These commands modify how the relay acts by either modifying the resulting quote, or changing where" +
                        " and how the quote is sent.")
                    .AddField("`-c | --channel <channel ID>` REQUIRED (unless latched)", "```http\n" +
                        $"Usage        :: -c 776083595299127296\n" +
                        $"Usage        :: --channel 776083595299127296\n" +
                        $"Channel ID   :: ID of the channel to send the message to." +
                        $"\n```")
                    .AddField("`-a | --author <new author>` OPTIONAL", "```http\n" +
                        $"Usage        :: -a \"Cloud Bot\"\n" +
                        $"Usage        :: --author \"Cloud Bot\"\n" +
                        $"New Author   :: Replaces the author of the quote with the next argument. Use \" around multi-word" +
                        $" authors.\n" +
                        $"\n```")
                    .AddField("`-s | --saved <new saved by message>` OPTIONAL", "```http\n" +
                        $"Usage        :: -s \"Someone saved this.\"\n" +
                        $"Usage        :: --saved \"Someone saved this.\"\n" +
                        $"New message  :: Replaces the saved by message of the quote with the next argument. Use \" around multi-word" +
                        $" messages.\n" +
                        $"\n```")
                    .AddField("`-i | --image [Image URL]` OPTIONAL", "```http\n" +
                        $"Usage        :: -i \"https://source.com/file.png\"\n" +
                        $"Usage        :: --image \"https://source.com/file.png\"\n" +
                        $"Image URL    :: Image URL to use. Or, attach an image to the command message.\n" +
                        $"\n```")
                    .AddField("`-C | --color <color>` OPTIONAL", "```http\n" +
                        $"Usage        :: -c #6fa9de\n" +
                        $"Usage        :: --color \"64, 51, 83\"\n" +
                        $"Color        :: Color to set the embed to. Must be a Hexadecimal value with an optial #, or an RGB value, where each color value" +
                        $" is between 0 and 255 and is spearated by commas.\n" +
                        $"\n```")
                    .AddField("NON-RELAY Commands", 
                        "Commands in the NON-RELAY category will not send a message to the relay if they are in the" +
                        $" command string. All other arguments will be ignored, and the command will imediatly stop execution after the first" +
                        $" NON-RELAY command runs. These commands are used to setup configuration options for the user of the relay.")
                    .AddField("`-h | --help`", "```http\n" +
                        $"Usage        :: -h\n" +
                        $"Usage        :: --help\n" +
                        $"Result       :: Sends this help embed." +
                        $"\n```")
                    .AddField("`-L | --latch <latch to> <latch mode>`", "```http\n" +
                        $"Usage        :: -L ALL LAST\n" +
                        $"Usage        :: --latch ALL LAST\n" +
                        $"Latch To     :: Specify an option from RELAY commands to latch to. Use the command name," +
                        $" ex: -a, --author, or --image. Optionally, use ALL to set the latch option for all RELAY commands.\n" +
                        $"Latch Mode   :: Select a mode from these options: LAST, DEFAULT, NONE." +
                        $" See the Latch Mode section for more information.\n" +
                        $"Result       :: Sends a latch confirmation message." +
                        $"\n```")
                    .AddField("`--shutdown`", "```http\n" +
                        $"Usage        :: --shutdown\n" +
                        $"Result       :: Stops the relay." +
                        $"\n```")
                    .AddField("Final Notes",
                        "The following are not commands, but important information for using the bot.")
                    .AddField("`Latch Mode`", 
                        "Avalible Modes: `LAST`, `DEFAULT`, `NONE`\n" +
                        "```http\n" +
                        "LAST    :: Latches relay changes to the last changed value. All messages after the value" +
                        " is changed will use the same value.\n" +
                        "DEFAULT :: Latches customization to a deafult. If the relay option is not sepcified," +
                        " it uses the default value.\n" +
                        "NONE    :: Disables latching, and uses system default if no relay is provided." +
                        "\n```");
        }
    }
}
