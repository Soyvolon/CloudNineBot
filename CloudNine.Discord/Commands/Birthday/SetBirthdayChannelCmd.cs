using System.Threading.Tasks;

using CloudNine.Discord.Utilities;

using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using DSharpPlus.SlashCommands.Attributes;

namespace CloudNine.Discord.Commands.Birthday
{
    public partial class BirthdayCommands : SlashCommandBase
    {
        [SlashCommand("channel", "Sets the birthday channel for this server.")]
        [SlashRequireGuild]
        [SlashRequireUserPermissions(Permissions.ManageGuild)]
        public async Task SetBirthdayChannelAsync(InteractionContext ctx,
            [Option("Channel", "Channel to set.")]
            DiscordChannel channel)
        {
            await ctx.CreateResponseAsync(DSharpPlus.InteractionResponseType.DeferredChannelMessageWithSource);
            _birthdays.UpdateBirthdayChannel(ctx.Guild.Id, channel);
            await Respond($"Set the birthday channel to {channel.Mention}.");
        }
    }
}
