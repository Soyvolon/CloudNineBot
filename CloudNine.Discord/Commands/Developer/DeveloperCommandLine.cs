using System;
using System.Linq;
using System.Threading.Tasks;

using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;

namespace CloudNine.Discord.Commands.Developer
{
    public class DeveloperCommandLine : BaseCommandModule
    {
        [Command("run")]
        [Description("Runs a developer command.")]
        [RequireOwner]
        public async Task DeveloperCommandLineParserAsync(CommandContext ctx, params string[] args)
        {
            try
            {
                if(args.Length < 1)
                {
                    await ctx.RespondAsync("No command entered.");
                }
                else if (args[0] == "dbupdate")
                {
                    await DbUpdateCommandAsync(ctx, args[1..]);
                }
                else
                {
                    await ctx.RespondAsync("command not found.");
                }
            }
            catch (Exception ex)
            {
                await ctx.RespondAsync($"Command errored:\n{ex.Message}");
            }
        }

        private async Task DbUpdateCommandAsync(CommandContext ctx, params string[] args)
        {

        }
    }
}
