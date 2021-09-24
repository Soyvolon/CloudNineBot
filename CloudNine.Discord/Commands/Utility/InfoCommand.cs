using System.Linq;
using System.Threading.Tasks;

using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;

namespace CloudNine.Discord.Commands.Utility
{
    public class InfoCommand : SlashCommandBase
    {
        private readonly DiscordShardedClient _client;

        public InfoCommand(DiscordShardedClient client)
        {
            _client = client;
        }

        [SlashCommand("about", "Shows info about Cloud Nine Bot")]
        public async Task InfoCommandAsync(InteractionContext ctx)
        {
            var embed = new DiscordEmbedBuilder();
            embed.WithColor(CommandModule.Color_Cloud)
                .WithTitle("Cloud Nine Bot")
                .WithDescription($"Server Count: {_client.ShardClients.Sum(x => x.Value.Guilds.Count)}\n" +
                $"Current Shard: {ctx.Client.ShardId}\n" +
                $"\n" +
                $"Check out our [Documentation](https://docs.andrewbounds.com/)\n" +
                $"Check out our [Dashboard](https://andrewbounds.com/)\n\n" +
                $"Want in-bot help? Use `/about`");

            await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource,
                new DiscordInteractionResponseBuilder()
                    .AddEmbed(embed));
        }
    }
}
