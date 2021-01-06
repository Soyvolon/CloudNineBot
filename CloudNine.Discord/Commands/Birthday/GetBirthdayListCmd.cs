using System.IO;
using System.Threading.Tasks;

using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.Exceptions;

namespace CloudNine.Discord.Commands.Birthday
{
    public class GetBirthdayListCmd : CommandModule
    {
        [Command("bdaylist")]
        [RequireGuild]
        [Aliases("brithdays")]
        [Description("Sends you a list of the birthdays from the server you run this command on to your DMs.")]
        public async Task GetBirthdayListAsync(CommandContext ctx, [RemainingText] string prams)
        {
            string str = $"{ctx.Guild.Name}'s Bday List:";

            var data = DiscordBot.Bot.Birthdays.GetAllBirthdaysForServer(ctx.Guild.Id, !(prams is null) && prams.ToLower().StartsWith("s"));

            if (data is null)
            {
                await ctx.RespondAsync("There are no birthdays on this server! :sob:").ConfigureAwait(false);
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

        private async Task SendText(CommandContext ctx, string str, DiscordDmChannel dm)
        {
            try
            {
                await dm.SendMessageAsync(str).ConfigureAwait(false);
            }
            catch (UnauthorizedException)
            {
                await SendDM(ctx, str, dm).ConfigureAwait(false);
            }

            await ctx.RespondAsync("We sent you a DM!").ConfigureAwait(false);
        }

        private async Task SendDM(CommandContext ctx, string str, DiscordDmChannel dm)
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

            await ctx.RespondAsync("We sent you a DM!").ConfigureAwait(false);
        }
    }
}
