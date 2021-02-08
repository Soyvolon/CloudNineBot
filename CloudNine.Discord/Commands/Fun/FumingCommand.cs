using System.Threading.Tasks;

using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;

namespace CloudNine.Discord.Commands.Fun
{
    public class FumingCommand : CommandModule
    {
        [Command("fuming")]
        [Description("Sparkle shortcut for the word fuming")]
        [RequireUserPermissions(Permissions.AccessChannels)]
        public async Task FumingCommandAsync(CommandContext ctx)
        {
            var cnext = ctx.Client.GetCommandsNext();
            var cmd = cnext.FindCommand("sparkle fuming", out var args);
            var fctx = cnext.CreateFakeContext(ctx.User, ctx.Channel, $"{ctx.Prefix}sparkle fuming", ctx.Prefix, cmd, args);
            await cnext.ExecuteCommandAsync(fctx);
        }
    }
}
