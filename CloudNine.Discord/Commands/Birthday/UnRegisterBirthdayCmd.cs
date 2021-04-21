using System.Threading.Tasks;

using CloudNine.Discord.Utilities;

using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;

namespace CloudNine.Discord.Commands.Birthday
{
    public class UnRegisterBirthdayCmd : CommandModule
    {
        private readonly BirthdayManager _birthdays;

        public UnRegisterBirthdayCmd(BirthdayManager birthdays)
        {
            _birthdays = birthdays;
        }

        [Command("remove")]
        [Aliases("unregister")]
        [RequireGuild]
        [Description("Removes you from the birthday list on this server.")]
        public async Task UnregisterBirthdayAsync(CommandContext ctx)
        {
            if (_birthdays.RemoveBirthday(ctx.Guild.Id, ctx.Member.Id))
                await ctx.RespondAsync("Birthday removed sucsessfuly!").ConfigureAwait(false);
            else
                await ctx.RespondAsync("You don't have a birthday registered on this server.").ConfigureAwait(false);
        }
    }
}