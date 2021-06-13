using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using CloudNine.Core.Configuration;
using CloudNine.Core.Database;
using CloudNine.Core.Extensions;

using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands.Attributes;
using DSharpPlus.SlashCommands.Entities;
using DSharpPlus.SlashCommands.Entities.Builders;

using Microsoft.Extensions.DependencyInjection;

namespace CloudNine.Discord.Commands.Slash
{
    public class AddQuoteCommand : BaseSlashCommandModule
    {
        public AddQuoteCommand(IServiceProvider p) : base(p) { }

        [SlashCommand("addquote", 1)]
        [Description("Adds a new quote to this server.")]
        public async Task GetQuoteCommandAsync(InteractionContext ctx,
            [Description("Author of the quote")]
            DiscordUser author,
            
            [Description("Contents of the quote")]
            string quote)
        {
            var member = await ctx.Interaction.Guild.GetMemberAsync(ctx.Interaction.User.Id);

            var perms = member.PermissionsIn(ctx.Interaction.Channel);

            if (perms.HasPermission(Permissions.ManageMessages))
            {
                var _client = _services.GetRequiredService<DiscordShardedClient>();

                DiscordClient? shard = null;
                foreach (var s in _client.ShardClients.Values)
                {
                    if (s.Guilds.TryGetValue(ctx.Interaction.GuildId ?? 0, out _))
                    {
                        shard = s;
                    }
                }

                if(shard is null)
                {
                    await ctx.ReplyAsync(new InteractionResponseBuilder()
                        .WithType(InteractionResponseType.ChannelMessageWithSource)
                        .WithData(new InteractionApplicationCommandCallbackDataBuilder()
                            .WithEmbed(new DiscordEmbedBuilder()
                                .WithDescription("No guild found. You cant add a quote if you aren't in a guild.")
                                .WithColor(DiscordColor.Red).Build())
                            ).Build());
                }

                var cnext = shard.GetCommandsNext();

                var contents = $"addquote {quote} --author \"{author.Username}\"";

                var cmd = cnext.FindCommand(contents, out string raw);

                var fake = cnext.CreateFakeContext(ctx.Interaction.User, ctx.Interaction.Channel, contents, "c!", cmd, raw);

                await cnext.ExecuteCommandAsync(fake);

                await ctx.ReplyAsync(new InteractionResponseBuilder()
                    .WithType(InteractionResponseType.ChannelMessageWithSource)
                    .WithData(new InteractionApplicationCommandCallbackDataBuilder()
                        .WithEmbed(new DiscordEmbedBuilder()
                            .WithDescription("Quote Added!")
                            .WithColor(DiscordColor.DarkGreen).Build())
                        ).Build());
            }
            else
            {
                await ctx.ReplyAsync(new InteractionResponseBuilder()
                    .WithType(InteractionResponseType.ChannelMessageWithSource)
                    .WithData(new InteractionApplicationCommandCallbackDataBuilder()
                        .WithEmbed(new DiscordEmbedBuilder()
                            .WithDescription("You don't have permission to add a new quote!")
                            .WithColor(DiscordColor.Red).Build())
                        ).Build());
            }
        }

    }
}
