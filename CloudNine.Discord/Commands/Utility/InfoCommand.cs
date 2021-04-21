using System.Linq;
using System.Threading.Tasks;

using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;

namespace CloudNine.Discord.Commands.Utility
{
    public class InfoCommand : CommandModule
    {
        private readonly DiscordShardedClient _client;

        public InfoCommand(DiscordShardedClient client)
        {
            _client = client;
        }

        [Command("info")]
        [Description("Shows info about Cloud Nine Bot")]
        [Aliases("about")]
        public async Task InfoCommandAsync(CommandContext ctx)
        {
            var embed = new DiscordEmbedBuilder();
            embed.WithColor(Color_Cloud)
                .WithTitle("Cloud Nine Bot")
                .WithDescription($"Server Count: {_client.ShardClients.Sum(x => x.Value.Guilds.Count)}\n" +
                $"Current Shard: {ctx.Client.ShardId}\n" +
                $"\n" +
                $"Check out our [Documentation](https://docs.andrewbounds.com/)\n" +
                $"Check out our [Dashboard](https://andrewbounds.com/)\n\n" +
                $"Want in-bot help? Use `{ctx.Prefix}help`");

            await ctx.RespondAsync(embed);
        }
    }
}
