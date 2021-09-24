using System.Threading.Tasks;

using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;

namespace CloudNine.Discord.Commands.Utility
{
    public class InviteCommand : ApplicationCommandModule
    {
        [SlashCommand("invite", "Invite Cloud Bot to your server!")]
        public async Task ExampleCommandAsync(InteractionContext ctx)
            => await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource,
                new DiscordInteractionResponseBuilder()
                .WithContent("Here is an [invite link](https://discord.com/api/oauth2/authorize?client_id=750486299789754389&permissions=388176&redirect_uri=https%3A%2F%2Fandrewbounds.com%2Flogin&scope=bot%20applications.commands) just for you!"));
    }
}
