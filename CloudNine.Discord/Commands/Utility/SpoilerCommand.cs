using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

namespace CloudNine.Discord.Commands.Utility
{
    public class SpoilerCommand : BaseCommandModule
    {
        private readonly HttpClient _client;

        public SpoilerCommand(HttpClient client)
        {
            _client = client;
        }

        [Command("spoiler")]
        [Description("Makes any attachements have a spoiler.")]
        [Aliases("spoil")]
        public async Task SpoilerCommandAsync(CommandContext ctx)
        {
            await ctx.TriggerTypingAsync();
            Dictionary<string, Stream> streams = new();

            int i = 0;
            foreach(var att in ctx.Message.Attachments)
            {
                var extension = att.Url[(att.Url.LastIndexOf('.') + 1)..att.Url.Length];
                var name = $"SPOILER_{att.FileName}_{Path.GetRandomFileName()}.{extension}";
                streams.Add(name, await _client.GetStreamAsync(att.Url));
            }

            var builder = new DiscordMessageBuilder()
                .WithFiles(streams);

            await builder.SendAsync(ctx.Channel);

            await ctx.Message.DeleteAsync();
        }
    }
}
