using System.IO;
using System.Threading.Tasks;

using CloudNine.Discord.Utilities;

using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.Exceptions;
using DSharpPlus.SlashCommands;
using DSharpPlus.SlashCommands.Attributes;

namespace CloudNine.Discord.Commands.Birthday
{
    public partial class BirthdayCommands : SlashCommandBase
    {
        [SlashCommand("list", "Sends you a list of the birthdays from the server you run this command on to your DMs.")]
        [SlashRequireGuild]
        public async Task GetBirthdayListAsync(InteractionContext ctx, 
            [Option("Upcoming", "True will sort the birthdays by the upcoming birthdays.")]
            bool ordered = true)
        {
            await ctx.CreateResponseAsync(DSharpPlus.InteractionResponseType.DeferredChannelMessageWithSource);

            string str = $"{ctx.Guild.Name}'s Bday List:";

            var data = _birthdays.GetAllBirthdaysForServer(ctx.Guild.Id, ordered);

            if (data is null)
            {
                await Respond("There are no birthdays on this server! :sob:");
                return;
            }

            foreach (var date in data)
            {
                str += $"\n\nBirthdays on: {date.Key.Date:dd MMMM}:";

                foreach (var bday in date.Value)
                {
                    if (ctx.Guild.Members.TryGetValue(bday, out DiscordMember? m))
                    {
                        str += $"\n{m.Nickname} | {m.Username}#{m.Discriminator}";
                    }
                    else
                    {
                        str += $"<@{bday}> | {bday}";
                    }
                }
            }

            var dm = await ctx.Member.CreateDmChannelAsync();

            if (str.Length > 2000)
            {
                await SendDM(ctx, str, dm).ConfigureAwait(false);
            }
            else
            {
                await SendText(ctx, str, dm).ConfigureAwait(false);
            }
        }

        private async Task SendText(InteractionContext ctx, string str, DiscordDmChannel dm)
        {
            try
            {
                await dm.SendMessageAsync(str).ConfigureAwait(false);
            }
            catch (UnauthorizedException)
            {
                await SendDM(ctx, str, dm).ConfigureAwait(false);
            }

            await Respond("We sent you a DM!");
        }

        private async Task SendDM(InteractionContext ctx, string str, DiscordDmChannel dm)
        {
            FileStream fs = new FileStream($"birthdays-{ctx.User.Id}.txt", FileMode.OpenOrCreate);
            StreamWriter sr = new StreamWriter(fs);

            await sr.WriteAsync(str).ConfigureAwait(false);
            await sr.FlushAsync().ConfigureAwait(false);

            var path = fs.Name;

            await sr.DisposeAsync().ConfigureAwait(false);
            await fs.DisposeAsync().ConfigureAwait(false);

            fs = new FileStream(path, FileMode.Open);

            try
            {
                await dm.SendMessageAsync(new DiscordMessageBuilder()
                    .WithFile(fs)
                    .WithContent($"Birthdays for {ctx.Guild.Name}"));
            }
            catch (UnauthorizedException)
            {
                await fs.DisposeAsync().ConfigureAwait(false);
                fs = new FileStream(path, FileMode.Open);

                await ctx.Channel.SendMessageAsync(new DiscordMessageBuilder()
                    .WithFile(fs)
                    .WithContent("Sending the file here because we can't DM it to you!"));

                await fs.DisposeAsync().ConfigureAwait(false);

                File.Delete(path);

                return;
            }

            await fs.DisposeAsync().ConfigureAwait(false);

            File.Delete(path);

            await Respond("We sent you a DM!");
        }
    }
}
