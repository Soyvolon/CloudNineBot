using System.Threading.Tasks;

using CloudNine.Discord.Utilities;

using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.SlashCommands;
using DSharpPlus.SlashCommands.Attributes;

namespace CloudNine.Discord.Commands.Birthday
{
    public partial class BirthdayCommands : SlashCommandBase
    {
        [SlashCommand("remove", "Removes you from the birthday list on this server.")]
        [SlashRequireGuild]
        public async Task UnregisterBirthdayAsync(InteractionContext ctx)
        {
            await ctx.CreateResponseAsync(DSharpPlus.InteractionResponseType.DeferredChannelMessageWithSource);
            if (_birthdays.RemoveBirthday(ctx.Guild.Id, ctx.User.Id))
                await Respond("Birthday removed sucsessfuly!");
            else
                await Respond("You don't have a birthday registered on this server.");
        }
    }
}