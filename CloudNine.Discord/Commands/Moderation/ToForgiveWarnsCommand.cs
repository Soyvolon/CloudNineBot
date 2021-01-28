using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using CloudNine.Core.Database;
using CloudNine.Core.Moderation;

using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;

using Microsoft.Extensions.DependencyInjection;

namespace CloudNine.Discord.Commands.Moderation
{
    public class ToForgiveWarnsCommand : CommandModule
    {
        private readonly IServiceProvider _services;

        public ToForgiveWarnsCommand(IServiceProvider services)
            => _services = services;

        [Command("warnstoforgive")]
        [Description("Lists the warns that are eligible for forgivness.")]
        [Aliases("toforgive", "toreview")]
        [RequireUserPermissions(Permissions.ManageMessages)]
        public async Task ToForgiveWarnsCommandAsync(CommandContext ctx)
        {
            var db = _services.GetRequiredService<CloudNineDatabaseModel>();
            var mod = await db.FindAsync<ModCore>(ctx.Guild.Id);

            if (mod is null)
                await RespondError("No eligible warnings.");
            else
            {
                var warns = mod.WarnSet.Where((x) =>
                {
                    if (x.Forgiven || x.NotForgiven) return false;

                    var now = DateTime.UtcNow;
                    if(x.IgnoreUntil is not null)
                    {
                        if (x.IgnoreUntil.Value.CompareTo(now) is 1) return false;
                    }
                    else if (mod.ForgiveAfter != TimeSpan.Zero)
                    {
                        var remainder = x.CreatedOn - mod.ForgiveAfter;
                        if (remainder.CompareTo(now) is -1 or 0) return false;
                    }

                    return true;
                }).ToList();

                warns.Sort((x, y) => x.CreatedOn.CompareTo(y.CreatedOn));

                DiscordEmbedBuilder res = ModBase();
                res.WithTitle("Warnings up for Review")
                    .WithDescription("THe following is a list of warnings that are eligible as they have either passed thier" +
                    " delay date or are newly eligible. This excludes warns that are: Not forgiven, Forgiven, or too new to be eligible.\n\n" +
                    $"Warns up for review: {warns.Count}");

                int batchStart = 1;
                int count = 0;
                int chars = 0;
                List<string> data = new();
                foreach(var w in warns)
                {
                    if(chars > 1900)
                    {
                        res.AddField($"Eligible Warns ({batchStart} to {count})", string.Join("\n\n", data));
                        count++;
                        batchStart = count;
                        chars = 0;
                        data.Clear();
                    }

                    string set = $"`{w.Key}` - `{w.UserId}`: {w.Message}";
                    chars += set.Length;
                    data.Add(set);
                    count++;
                }

                if(data.Count > 0)
                    res.AddField($"Eligible Warns ({batchStart} to {count})", string.Join("\n\n", data));

                await ctx.RespondAsync(res);
            }
        }
    }
}
