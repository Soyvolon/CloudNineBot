using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

using CloudNine.Core.Database;
using CloudNine.Core.Http;
using CloudNine.Core.Multisearch;
using CloudNine.Core.Multisearch.Builders;

using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;

using Microsoft.Extensions.DependencyInjection;

namespace CloudNine.Discord.Commands.Multiserach
{
    public class SearchCommand : MultisearchCommandBase
    {
        public SearchCommand(IServiceProvider s) : base(s) { }

        [GroupCommand]
        [Description("Search for a fanfic.")]
        public async Task SearchCommandAsync(CommandContext ctx, 
            [Description("Search arguments.")]
            params string[] args)
        {
            var db = _services.GetRequiredService<CloudNineDatabaseModel>();
            var searchUser = await db.FindAsync<MultisearchUser>(ctx.Member.Id);

            if (searchUser is null)
            {
                searchUser = new()
                {
                    Id = ctx.Member.Id
                };

                await db.AddAsync(searchUser);
                await db.SaveChangesAsync();
            }

            var search = new SearchBuilder();
            search.SetBasic(args[0]);

            var res = await searchUser.NewSearch(_services.GetRequiredService<BrowserClient>(), search.Build());
            for (int i = 0; i < 10; i++)
            {
                var embeds = res[i].GetDiscordEmbeds(new());

                foreach (var e in embeds)
                    await ctx.RespondAsync(embed: e);

                await Task.Delay(TimeSpan.FromSeconds(0.5));
            }
        }
    }
}
