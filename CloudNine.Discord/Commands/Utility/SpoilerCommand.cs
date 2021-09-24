using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;

using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

namespace CloudNine.Discord.Commands.Utility
{
    public class SpoilerCommand : ApplicationCommandModule
    {
        private readonly HttpClient _client;

        public SpoilerCommand(HttpClient client)
        {
            _client = client;
        }

        [ContextMenu(ApplicationCommandType.MessageContextMenu, "spoiler")]
        public async Task SpoilerCommandAsync(ContextMenuContext ctx)
        {
            if (ctx.TargetMessage.Author.Id == ctx.User.Id)
            {
                Dictionary<string, Stream> streams = new();

                int i = 0;
                foreach (var att in ctx.TargetMessage.Attachments)
                {
                    var extension = att.Url[(att.Url.LastIndexOf('.') + 1)..att.Url.Length];
                    var name = $"SPOILER_{att.FileName}_{Path.GetRandomFileName()}.{extension}";
                    streams.Add(name, await _client.GetStreamAsync(att.Url));
                }

                if (streams.Count > 0)
                {

                    await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource,
                        new DiscordInteractionResponseBuilder()
                        .AddFiles(streams));

                    await ctx.TargetMessage.DeleteAsync();
                }
                else
                {
                    await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource,
                    new DiscordInteractionResponseBuilder()
                    .WithContent("No attachments to spoiler!"));
                }
            }
            else
            {
                await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource,
                    new DiscordInteractionResponseBuilder()
                    .WithContent("You must have posted the original image to spoiler it!"));
            }
        }
    }
}
