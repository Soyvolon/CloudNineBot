using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
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
using DSharpPlus.SlashCommands;

using Microsoft.Extensions.Logging;

namespace CloudNine.Discord.Services
{
    public class QuoteService
    {
        public static Dictionary<string, object> RelayDefaults { get; private set; } = new Dictionary<string, object>
        {
            { "--author", "Cloud Bot" },
            { "--saved", "Cloud Bot" },
            { "--image", "" },
            { "--color",  CommandModule.Color_Cloud }
        };

        public static HashSet<string> RelayCommands { get; private set; } = new HashSet<string>()
        {
            "-c", "--channel",
            "-a", "--author",
            "-s", "--saved",
            "-i", "--image",
            "-C", "--color",
            "-t", "--time",
        };

        public static Dictionary<string, string> ShortToLongCommands { get; private set; } = new Dictionary<string, string>()
        {
            { "-c", "--channel" },
            { "-a", "--author" },
            { "-s", "--saved" },
            { "-i", "--image" },
            { "-C", "--color" },
            { "-t", "--time" }
        };

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
            public QuoteData Data { get; } = new QuoteData();
        }

        public class QuoteData
        {
            // Latch Variables
            public ulong? ChannelId { get; set; }
            public string? Author { get; set; }
            public string? SavedBy { get; set; }
            public string? ImageUrl { get; set; }
            public DiscordColor? Color { get; set; }
            public DateTime? Time { get; set; }
            public string? CustomId { get; set; }
            public string? Content { get; set; }
        }

        public ConcurrentDictionary<ulong, Relay> ActiveLinks { get; init; }

        private readonly ILogger _logger;
        private ConcurrentDictionary<MessageCreateEventArgs, Tuple<Task, CancellationTokenSource>> RunningRelays;

        public QuoteService(ILogger<QuoteService> logger)
        {
            this._logger = logger;

            ActiveLinks = new ConcurrentDictionary<ulong, Relay>();
            RunningRelays = new ConcurrentDictionary<MessageCreateEventArgs, Tuple<Task, CancellationTokenSource>>();
        }

        public List<string> GetParamsString(string input)
        {
            return Regex.Matches(input, @"(['\""])(?<value>.+?)\1|(?<value>[^ ]+)")
                .Cast<Match>()
                .Select(m => m.Groups["value"].Value)
                .ToList();
        }

        public async Task<bool> TryOpenRelayAsync(DiscordUser source, DiscordChannel sourceChannel, 
            ulong destGuildId, char actionKey)
        {
            if (ActiveLinks.ContainsKey(source.Id)) return false;

            var client = await DiscordBot.Bot.GetClientForGuildId(destGuildId);

            if (client is null) return false;

            var guild = await client.GetGuildAsync(destGuildId);

            ActiveLinks[source.Id] = new Relay
            {
                Destination = guild,
                SourceId = sourceChannel.Id,
                ActionKey = actionKey
            };

            return true;
        }

        private async Task Relay_MessageCreatedAsync(DiscordClient source, InteractionContext e, string argsRaw)
        {
            if (ActiveLinks.TryGetValue(e.User.Id, out var r))
            {
                var args = GetParamsString(argsRaw);

                if (args.Count > 0 && args[0].StartsWith(r.ActionKey))
                {
                    if (args[0].Equals(r.ActionKey))
                        args.RemoveAt(0);
                    else if (args[0].Length > 1)
                        args[0] = args[0][1..];
                    else
                        return; // failed to separate the prefix. This should be impossible.

                    await SendQuoteRelay(e, r, args, source);
                }
            }
        }

        private async Task SendQuoteRelay(InteractionContext source, Relay relay, List<string> args, DiscordClient client)
        {
            var data = new QuoteData();

            List<string> content = new List<string>();

            #region Argument Parser

            for (int i = 0; i < args.Count; i++)
            {
                bool isArgRun = false;
                (i, data, isArgRun) = await ExecuteArgumentChecks(args, i, data, source);

                if (i == -1 || data is null) return;

                switch(args[i])
                {
                    #region NON-RELAY Commands
                    case "-h":
                    case "--help":
                        await source.Channel.SendMessageAsync(embed: GetQuoteRelayHelp(""));
                        return;

                    case "-L":
                    case "--latch":
                        if(args.Count <= i + 2)
                        {
                            await source.Channel.SendMessageAsync("Failed to parse `--latch`, not enough arguments.");
                        }
                        else
                        {
                            string command = args[++i].ToLower();
                            string mode = args[++i].ToLower();

                            if (command == "default")
                            {
                                var defaultData = new QuoteData();
                                for (_ = i; i < args.Count; i++)
                                {
                                    (i, defaultData, _) = await ExecuteArgumentChecks(args, i, data, source);
                                    if (i == -1 || data is null) return;
                                }

                                if (defaultData.ChannelId is not null)
                                {
                                    relay.Latches["--channel"] = LatchMode.Default;
                                    relay.Data.ChannelId = defaultData.ChannelId;

                                    await source.Channel.SendMessageAsync($"Set latching to `DEFAULT` for `--channel` with" +
                                        $" default:\n{defaultData.ChannelId}");
                                }

                                if (defaultData.Author is not null)
                                {
                                    relay.Latches["--author"] = LatchMode.Default;
                                    relay.Data.Author = defaultData.Author;

                                    await source.Channel.SendMessageAsync($"Set latching to `DEFAULT` for `--author` with" +
                                        $" default:\n{defaultData.Author}");
                                }

                                if (defaultData.SavedBy is not null)
                                {
                                    relay.Latches["--saved"] = LatchMode.Default;
                                    relay.Data.SavedBy = defaultData.SavedBy;

                                    await source.Channel.SendMessageAsync($"Set latching to `DEFAULT` for `--saved` with" +
                                        $" default:\n{defaultData.SavedBy}");
                                }

                                if (defaultData.ImageUrl is not null)
                                {
                                    relay.Latches["--image"] = LatchMode.Default;
                                    relay.Data.ImageUrl = defaultData.ImageUrl;

                                    await source.Channel.SendMessageAsync($"Set latching to `DEFAULT` for `--image` with" +
                                        $" default:\n{defaultData.ImageUrl}");
                                }

                                if (defaultData.Color is not null)
                                {
                                    relay.Latches["--color"] = LatchMode.Default;
                                    relay.Data.Color = defaultData.Color;

                                    await source.Channel.SendMessageAsync($"Set latching to `DEFAULT` for `--color` with" +
                                        $" default:\n{defaultData.Color}");
                                }

                                if (defaultData.Time is not null)
                                {
                                    relay.Latches["--time"] = LatchMode.Default;
                                    relay.Data.Time = defaultData.Time;

                                    await source.Channel.SendMessageAsync($"Set latching to `DEFAULT` for `--time` with" +
                                        $" default:\n{defaultData.Time}");
                                }
                            }
                            else if (RelayCommands.Contains(command) || command == "all")
                            {
                                List<string> commandsToLatch;
                                if (command == "all")
                                {
                                    commandsToLatch = RelayCommands.Where(x => x.StartsWith("--")).ToList();
                                }
                                else
                                {
                                    commandsToLatch = new List<string>();
                                    if (command.StartsWith("--"))
                                        commandsToLatch.Add(command);
                                    else
                                        commandsToLatch.Add(ShortToLongCommands[command]);
                                }
                                switch (mode)
                                {
                                    case "none":
                                        foreach (var c in commandsToLatch)
                                        {
                                            relay.Latches.TryRemove(c, out _);
                                        }

                                        await source.Channel.SendMessageAsync($"Set latching to `NONE` for {command}");
                                        break;

                                    case "last":
                                        foreach (var c in commandsToLatch)
                                        {
                                            relay.Latches[c] = LatchMode.Last;
                                        }

                                        await source.Channel.SendMessageAsync($"Set latching to `LAST` for {command}");
                                        break;

                                    default:
                                        await source.Channel.SendMessageAsync("Failed to parse `--latch`, mode not found.");
                                        break;
                                }
                            }
                            else
                            {
                                await source.Channel.SendMessageAsync("Failed to parse `--latch`, command not found or ALL not specified.");
                            }
                        }
                        return;

                    case "--shutdown":
                        await TryCloseRelayAsync(source.User);
                        await source.Channel.SendMessageAsync(embed: new DiscordEmbedBuilder()
                            .WithColor(DiscordColor.DarkRed)
                            .WithDescription("Relay Closed."));
                        return;
                    #endregion

                    default:
                        if(!isArgRun)
                            content.Add(args[i]);
                        break;
                }
            }

            #endregion

            // Make sure a channel object exsists.
            ExecuteParamAssignment("--channel", relay, RelayDefaults,
                data.ChannelId, d => data.ChannelId = (ulong?)d,
                relay.Data.ChannelId, r => relay.Data.ChannelId = (ulong?)r);

            if (data.ChannelId is null)
            {
                await source.Channel.SendMessageAsync("Missing required argument `--channel`");
                return;
            }

            Quote quote = new()
            {
                // Quote Message
                Content = string.Join(" ", content),
                Id = -2
            };

            // Author
            ExecuteParamAssignment("--author", relay, RelayDefaults,
                data.Author, d => quote.Author = (string)d,
                relay.Data.Author, r => relay.Data.Author = (string)r);
            // Saved By
            ExecuteParamAssignment("--saved", relay, RelayDefaults,
                data.SavedBy, d => quote.SavedBy = (string)d,
                relay.Data.SavedBy, r => relay.Data.SavedBy = (string)r);
            // Image
            ExecuteParamAssignment("--image", relay, RelayDefaults,
                data.ImageUrl, d => quote.Attachment = (string)d,
                relay.Data.ImageUrl, r => relay.Data.ImageUrl = (string)r);
            // Color
            ExecuteParamAssignment("--color", relay, RelayDefaults,
                data.Color, d => quote.Color = (DiscordColor)d,
                relay.Data.Color, r => relay.Data.Color = (DiscordColor)r);
            // Time
            ExecuteParamAssignment("--time", relay, RelayDefaults,
                data.Time, d => quote.SavedAt = (DateTime?)d,
                relay.Data.Time, r => relay.Data.Time = (DateTime?)r);

            await relay.Destination.GetChannel((ulong)data.ChannelId).SendMessageAsync(embed: quote.BuildQuote());
            await source.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource,
                new DiscordInteractionResponseBuilder()
                .WithContent($"Sent Relay at {DateTime.UtcNow.ToShortTimeString()}"));
        }

        public async Task<(int, QuoteData, bool)> ExecuteArgumentChecks(List<string> args, int i, QuoteData data, InteractionContext source)
        {
            bool argRun = false;
            switch(args[i])
            {
                #region RELAY Commands
                case "-c":
                case "--channel":
                    argRun = true;
                    if (args.Count <= i + 1)
                    {
                        await source.Channel.SendMessageAsync("Failed to parse `--channel`, not enough paramaters.");
                        return (-1, null, false);
                    }
                    else
                    {
                        string toParse = args[++i];
                        if (toParse.StartsWith("<#") && toParse.EndsWith(">"))
                            toParse = toParse[2..(toParse.Length - 1)];

                        if (ulong.TryParse(toParse, out var id))
                        {
                            data.ChannelId = id;
                        }
                        else
                        {
                            await source.Channel.SendMessageAsync("Failed to parse `--channel`, invalid ID.");
                            return (-1, null, false);
                        }
                    }
                    break;

                case "-a":
                case "--author":
                    argRun = true;
                    if (args.Count <= i + 1)
                    {
                        await source.Channel.SendMessageAsync("Failed to parse `--author`, not enough paramaters.");
                        return (-1, null, false);
                    }
                    else
                    {
                        var author = await TryParseUserId(args[++i], source);

                        if (author is null) return (-1, null, false);

                        data.Author = author;
                    }
                    break;

                case "-s":
                case "--saved":
                    argRun = true;
                    if (args.Count <= i + 1)
                    {
                        await source.Channel.SendMessageAsync("Failed to parse `--saved`, not enough paramaters.");
                        return (-1, null, false);
                    }
                    else
                    {
                        var author = await TryParseUserId(args[++i], source);

                        if (author is null) return (-1, null, false);

                        data.SavedBy = author;
                    }
                    break;

                case "-i":
                case "--image":
                    argRun = true;
                    string url = "";
                    try
                    {
                        var uri = new Uri(args[++i]);
                        url = uri.AbsoluteUri;
                    }
                    catch
                    {
                        await source.Channel.SendMessageAsync("Failed to parse `--image`, failed to get a valid URL.");
                        return (-1, null, false);
                    }

                    data.ImageUrl = url;
                    break;

                case "-C":
                case "--color":
                    argRun = true;
                    if (args.Count <= i + 1)
                    {
                        await source.Channel.SendMessageAsync("Failed to parse `--color`, not enough paramaters.");
                        return (-1, null, false);
                    }
                    else
                    {
                        string colorString = args[++i];
                        if (colorString.Contains(","))
                        {
                            var parts = colorString.Split(",", StringSplitOptions.RemoveEmptyEntries).ToArray();
                            if (parts.Length < 3)
                            {
                                await source.Channel.SendMessageAsync("Failed to parse `--color`, not enough vlaues for an RGB value.");
                                return (-1, null, false);
                            }

                            byte[] values = new byte[3];
                            for (int c = 0; c < values.Length; c++)
                            {
                                if (byte.TryParse(parts[c], out byte v))
                                {
                                    if (v > 255 || v < 0)
                                    {
                                        await source.Channel.SendMessageAsync($"Failed to parse `--color`, RGB value #{c} is not within 0 and 255.");
                                        return (-1, null, false);
                                    }

                                    values[c] = v;
                                }
                                else
                                {
                                    await source.Channel.SendMessageAsync($"Failed to parse `--color`, RGB value #{c} is not a number.");
                                    return (-1, null, false);
                                }
                            }

                            data.Color = new DiscordColor(values[0], values[1], values[2]);
                        }
                        else
                        {
                            if (colorString.Length == 6 || colorString.Length == 7)
                            {
                                if (colorString.StartsWith("#"))
                                    colorString = colorString[1..];

                                if (int.TryParse(colorString, System.Globalization.NumberStyles.HexNumber, null, out int result))
                                {
                                    // Try Catch this because a value with 7 hex numbers will pass the if statment
                                    // but fail the color creation.
                                    data.Color = new DiscordColor(result);
                                }
                                else
                                {
                                    await source.Channel.SendMessageAsync($"Failed to parse `--color`, could not parse the Hex value.");
                                    return (-1, null, false);
                                }
                            }
                            else
                            {
                                await source.Channel.SendMessageAsync($"Failed to parse `--color`, could not parse a Hex or RGB value..");
                                return (-1, null, false);
                            }
                        }
                    }
                    break;

                case "-t":
                case "--time":
                    argRun = true;
                    if (args.Count <= i + 1)
                    {
                        await source.Channel.SendMessageAsync("Failed to parse `--time`, not enough paramaters.");
                        return (-1, null, false);
                    }
                    else
                    {
                        if (DateTime.TryParse(args[++i], CultureInfo.InvariantCulture, DateTimeStyles.None, out var result))
                        {
                            data.Time = result;
                        }
                        else
                        {
                            await source.Channel.SendMessageAsync("Failed to parse `--time`, unable to parse date time string.");
                            return (-1, null, false);
                        }
                    }
                    break;

                case "--custom":
                    argRun = true;
                    if(args.Count <= i + 1)
                    {
                        await source.Channel.SendMessageAsync("Failed to parse `--custom`, not enough paramaters.");
                        return (-1, null, false);
                    }
                    else
                    {
                        data.CustomId = args[++i];
                    }
                    break;

                case "-m":
                case "--message":
                    if (args.Count <= i + 1)
                    {
                        await source.Channel.SendMessageAsync("Failed to parse `--message`, not enough paramaters.");
                        return (-1, null, false);
                    }
                    else
                    {
                        data.Content = args[++i];
                    }
                    break;
                    #endregion
            }

            return (i, data, argRun);
        }

        public void ExecuteParamAssignment(string propertyCommand, Relay r, Dictionary<string, object> defaults,
            object data, Action<object> assignData,
            object? latch, Action<object>? assignLatch)
        {
#pragma warning disable CS8604 // Possible null reference argument. -- this must be handled outside of this method.
            try
            {
                defaults.TryGetValue(propertyCommand, out object? d);

                // if the data is null (no command for this was entered)...
                if (data is null)
                {
                    // ... see if a latch exsists ...
                    if (r.Latches.TryGetValue(propertyCommand, out _))
                    { // ... and assigned the latched value to the data.
                        assignData(latch ?? d);
                    }
                    else
                    { // ... if it doesnt, assign the default to the data.
                        assignData(d);
                    }
                }
                else
                { // ... if the command was entered ...
                    if (r.Latches.TryGetValue(propertyCommand, out var mode))
                    { // ... and latch with the mode LAST exsists ...
                        if (mode == LatchMode.Last)
                            // ... assign the data to the latch property.
                            if(assignLatch is not null)
                                assignLatch(data);
                    }

                    assignData(data);
                }
#pragma warning restore CS8604 // Possible null reference argument.
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Param Assignement Failed");
                throw new ArgumentException("Failed to parse an argument", ex);
            }
        }

        private async Task<string?> TryParseUserId(string author, InteractionContext source)
        {
            var res = "";
            if (author.StartsWith("<@!") && author.EndsWith(">"))
            {
                res = author[3..(author.Length - 1)];
            }
            else if(author.StartsWith("<@"))
            {
                res = author[2..(author.Length - 1)];
            }
            else
            {
                return author;
            }

            if (ulong.TryParse(res, out ulong id))
            {
                try
                {
                    var user = await source.Channel.Guild.GetMemberAsync(id);
                    res = user.Username;
                }
                catch
                {
                    await source.Channel.SendMessageAsync("Failed to parse `--author`, invalid mention. Mentions must be from" +
                        " members on this server.");
                    return null;
                }
            }
            else
            {
                await source.Channel.SendMessageAsync("Failed to parse `--author`, invalid mention. Mentions must be from" +
                        " members on this server.");
                return null;
            }

            return res;
        }

        public Task<bool> TryCloseRelayAsync(DiscordUser source)
        {
            return Task.FromResult(ActiveLinks.TryRemove(source.Id, out _));
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
                        $"Color        :: Color to set the embed to. Must be a Hexadecimal value with an optional #, or an RGB value, where each color value" +
                        $" is between 0 and 255 and is spearated by commas.\n" +
                        $"\n```")
                    .AddField("`-t | --time <time>` OPTIONAL", "```http\n" +
                        $"Usage        :: -t 11/03/2020\n" +
                        $"Usage        :: --time \"11/03/2020 15:23\"\n" +
                        $"Time         :: Time to be displayed in the Saved By section.\n" +
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
                    .AddField("`-L | --latch <latch to> (<latch mode> | <defaults>)`", "```http\n" +
                        $"Usage        :: -L ALL LAST\n" +
                        $"Usage        :: --latch ALL LAST\n" +
                        $"Latch To     :: Specify an option from RELAY commands to latch to. Use the command name," +
                        $" ex: -a, --author, or --image. Optionally, use ALL to set the latch option for all RELAY commands," +
                        $" or DEFAULT to set defaults for commands. See the Latch Target section for more information.\n" +
                        $"Latch Mode   :: Select a mode from these options: LAST, NONE." +
                        $" See the Latch Mode section for more information.\n" +
                        $"Default      :: Sets the default for the command. Only used with the DEFAULT mode.\n" +
                        $"Result       :: Sends a latch confirmation message." +
                        $"\n```")
                    .AddField("`--shutdown` | `--abort`", "```http\n" +
                        $"Usage        :: --shutdown\n" +
                        $"Result       :: Stops the relay." +
                        $"\n```")
                    .AddField("Final Notes",
                        "The following are not commands, but important information for using the bot.")
                    .AddField("`Latch Target`", 
                        "Avalible Targets: Any command name, `ALL`, `DEFAULT`\n" +
                        "```http\n" +
                        "Commmand Name :: The command to set a latch for. Requires a Latch Mode.\n" +
                        "ALL           :: Targets every command. Requires a Latch Mode." +
                        " Any existing latches will be overwritten.\n" +
                        "DEFAULT       :: Latches customization to a deafult. If the relay option is not sepcified," +
                        " it uses the set value. This option ignores the Latch Mode variable and isntead uses string" +
                        " of RELAY commands to set defaults.\n" +
                        "\n```")
                    .AddField("`Latch Mode`", 
                        "Avalible Modes: `LAST`, `NONE`\n" +
                        "```http\n" +
                        "LAST    :: Latches relay changes to the last changed value. All messages after the value" +
                        " is changed will use the same value.\n" +
                        "NONE    :: Disables latching, and uses system default if no relay is provided." +
                        "\n```");
        }
    }
}
