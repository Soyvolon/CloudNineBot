using System.Threading.Tasks;

using CloudNine.Discord.Services;

using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity.Extensions;

namespace CloudNine.Discord.Commands.Devine
{
    public class OpenQuoteRelay : CommandModule
    {
        private readonly QuoteService _relay;

        public OpenQuoteRelay(QuoteService relay)
        {
            this._relay = relay;
        }

        [Command("quoterelay")]
        [Description("Starts running a new quote boardcaster from this channel, localized to this server.")]
        [Aliases("dev-qb")]
        [RequireUserPermissions(Permissions.ManageMessages)]
        [RequireGuild]
        [Hidden]
        public async Task OpenQuoteBroadcasterAsync(CommandContext ctx)
            => await OpenRelayAsync(ctx, ctx.Guild.Id);

        [Command("remotequoterelay")]
        [Description("Starts running a new quote boardcaster from this channel, localized to this server.")]
        [Aliases("dev-rqb")]
        [RequireDirectMessage]
        [Hidden]
        public async Task OpenRemoteQuoteBroadcasterAsync(CommandContext ctx,
            [Description("Discord guild to link to.")]
            DiscordGuild discordGuild)
            => await OpenRelayAsync(ctx, discordGuild.Id);

        [Command("remotequoterelay")]
        public async Task OpenRemoteQuoteBroadcasterAsync(CommandContext ctx,
            [Description("Discord guild ID to link to")]
            ulong guildId)
        {
            var res = await DiscordBot.Bot.GetClientForGuildId(guildId);
            if(res is not null)
            {
                await OpenRemoteQuoteBroadcasterAsync(ctx, await res.GetGuildAsync(guildId));
            }
            else
            {
                await RespondError("Failed to get a guild for the provided ID.");
            }
        }

        private async Task OpenRelayAsync(CommandContext ctx, ulong guildId)
        {
            if (_relay.ActiveLinks.ContainsKey(ctx.User.Id))
            {
                await RespondError($"You already have an open link!\n" +
                    $"Use `{ctx.Prefix}forcecloserelay` to force your open link to close.");
                return;
            }

            var interact = ctx.Client.GetInteractivity();

            var b = InteractBase()
                .WithTitle("Setup Relay:")
                .WithDescription("Welcome to the Cloud Bot relay creator. This will create a realy from this channel " +
                "to any other channel we can see on this server!\n\n" +
                "To start, pelase enter your `Action Key`. This is a single character and will be used to trigger relay actions.\n" +
                "It must be exactly one `(1)` character long. An example is `;`.\n\n" +
                "Type `abort` at any time to abort this setup.");

            await ctx.RespondAsync(embed: b);

            char actionKey;
            string input = "not a char";
            int c = 0;
            do
            {
                if (c++ > 0)
                    await RespondError("Failed to parse an `Action Key`. Please make sure you are inputing " +
                        "a single character. Type `abort` to abort this setup.");

                var res = await interact.WaitForMessageAsync(x => x.Author == ctx.Message.Author);

                if (res.TimedOut)
                {
                    await RespondError("Operation Timed Out. Failed to input `Action Key`");
                    return;
                }

                input = res.Result.Content;

                if (input == "abort")
                {
                    await Respond("Aborting...");
                    return;
                }

            } while (!char.TryParse(input, out actionKey));

            b = InteractBase()
                .WithTitle("Creating Relay:")
                .WithDescription($"Your `Action Key` has been set to `{actionKey}`. Don't forget this, as the relay will" +
                $" only respond to that character.\n\n");

            await ctx.RespondAsync(embed: b);

            bool result = await _relay.TryOpenRelayAsync(ctx.User, ctx.Channel, guildId, actionKey);

            if (result)
            {
                b = InteractBase()
                    .WithTitle("Relay Created!")
                    .WithDescription("You are good to go! All normal commands will still work in this channel, but" +
                    " now relay actions will work as well. For more information about relay actions, please use " +
                    $"`{actionKey}--help` or `{ctx.Prefix}relayhelp`");

                await ctx.RespondAsync(embed: b);
            }
            else
            {
                await RespondError("Failed to initalize a new relay. Please notify Soyvolon#8016 (<@133735496479145984>) " +
                    "if this problem persist.");
            }
        }
    }
}
