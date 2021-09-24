using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CloudNine.Discord.Commands
{
    public class SlashCommandBase : ApplicationCommandModule
    {
        public static readonly DiscordColor Color_Cloud = new DiscordColor(0x3498db);
        public static readonly DiscordColor Color_Warn = new DiscordColor(0xe07c10);
        public static readonly DiscordColor Color_Search = DiscordColor.Aquamarine;

        protected BaseContext Ctx { get; set; }

        public override async Task<bool> BeforeContextMenuExecutionAsync(ContextMenuContext ctx)
        {
            Ctx = ctx;
            return await base.BeforeContextMenuExecutionAsync(ctx);
        }

        public override Task<bool> BeforeSlashExecutionAsync(InteractionContext ctx)
        {
            Ctx = ctx;
            return base.BeforeSlashExecutionAsync(ctx);
        }


        public async Task RespondAsync(DiscordEmbedBuilder embed)
        {
            await Ctx.FollowUpAsync(new DiscordFollowupMessageBuilder()
                .AddEmbed(embed));
        }

        public async Task Respond(string response)
        {
            await Ctx.FollowUpAsync(new DiscordFollowupMessageBuilder()
                .WithContent(response));
        }

        public async Task RespondWarn(string response)
        {
            await Ctx.FollowUpAsync(new DiscordFollowupMessageBuilder()
                .AddEmbed(ModBase().WithDescription(response)));
        }

        public async Task RespondError(string response)
        {
            await Ctx.FollowUpAsync(new DiscordFollowupMessageBuilder()
                .AddEmbed(ErrorBase().WithDescription(response)));
        }

        public static DiscordEmbedBuilder ErrorBase()
        {
            return new DiscordEmbedBuilder()
                .WithColor(DiscordColor.Red);
        }

        public static DiscordEmbedBuilder ModBase()
        {
            return new DiscordEmbedBuilder()
                .WithColor(Color_Warn);
        }

        public static DiscordEmbedBuilder InteractBase()
        {
            return new DiscordEmbedBuilder()
                .WithColor(Color_Cloud);
        }

        public static DiscordEmbedBuilder SuccessBase()
        {
            return new DiscordEmbedBuilder()
                .WithColor(Color_Cloud);
        }

        public async Task InteractTimeout(string message = "Interactivty Timed Out.")
        {
            var embed = ErrorBase()
                .WithDescription(message);

            await Ctx.FollowUpAsync(new DiscordFollowupMessageBuilder()
                .AddEmbed(embed));
        }
    }
}
