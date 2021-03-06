﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using CloudNine.Core.Configuration;
using CloudNine.Core.Database;
using CloudNine.Core.Extensions;

using DSharpPlus;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.SlashCommands.Attributes;
using DSharpPlus.SlashCommands.Entities;
using DSharpPlus.SlashCommands.Entities.Builders;

using Microsoft.Extensions.DependencyInjection;

namespace CloudNine.Discord.Commands.Slash
{
    public class GetQuoteCommand : BaseSlashCommandModule
    {
        public GetQuoteCommand(IServiceProvider p) : base(p) { }

        [SlashCommand("quote", 1)]
        [Description("Gets a random quote from this server.")]
        public async Task GetQuoteCommandAsync(InteractionContext ctx)
        {
            var db = _services.GetRequiredService<CloudNineDatabaseModel>();

            var config = await db.FindAsync<DiscordGuildConfiguration>(ctx.Interaction.GuildId);

            if (config is not null)
            {
                var quoteId = config.Keys.Random();
                if (config.Quotes.TryGetValue(quoteId, out var quote))
                {
                    var embed = quote.UseQuote();

                    db.Update(config);
                    await db.SaveChangesAsync();

                    await ctx.ReplyAsync(new InteractionResponseBuilder()
                        .WithType(InteractionResponseType.ChannelMessageWithSource)
                        .WithData(new InteractionApplicationCommandCallbackDataBuilder()
                            .WithEmbed(embed))
                        .Build());

                    return;
                }
            }

            await ctx.ReplyAsync("No quotes found.");
        }
    }
}
