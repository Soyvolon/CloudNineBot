using System.Linq;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;

using CloudNine.Core.Database;
using CloudNine.Core.Moderation;

using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;

using static CloudNine.Core.Moderation.ModCore;
using System;

namespace CloudNine.Discord.Commands.Moderation
{
    public class AddWarnCommand : CommandModule
    {
        private readonly CloudNineDatabaseModel _database;

        public AddWarnCommand(CloudNineDatabaseModel database)
        {
            this._database = database;
        }

        [Command("warn")]
        [Description("Warns a user against an action. Use --notify to send the warn message to the user.")]
        [RequireUserPermissions(Permissions.ManageRoles)]
        public async Task AddWarnCommandAsync(CommandContext ctx,
            [Description("Member to log a warning for.")]
            DiscordMember toWarn,

            [Description("Warn message to record.")]
            [RemainingText]
            string warnMessage)
        {
            string msg;
            bool notify = false;
            if(warnMessage.Contains("--notify"))
            {
                notify = true;
                msg = warnMessage.Replace("--notify", string.Empty);
            }
            else
            {
                msg = warnMessage;
            }

            var mod = await _database.FindAsync<ModCore>(ctx.Guild.Id);

            if(mod is null)
            {
                mod = new ModCore(ctx.Guild.Id);
                await _database.AddAsync(mod);
                await _database.SaveChangesAsync();
            }

            try
            {
                if (await mod.AddWarn(toWarn.Id, msg))
                {
                    _database.Update(mod);
                    await _database.SaveChangesAsync();

                    if (notify)
                    {
                        var dm = await toWarn.CreateDmChannelAsync();
                        await dm.SendMessageAsync($"{msg}\n\n" +
                            $"*This is an automated message. Do not reply to this bot, your message will not been seen. " +
                            $"Please send questions to {ctx.User.Mention}*");
                    }

                    await RespondWarn($"Successfuly logged warning for user {toWarn.Mention}. {(notify ? "The user has been notified." : "")}");
                }
                else
                {
                    await RespondError("Failed to create a new warn or this warn has already been added.\n\n" +
                        "(This error should not be show, please notfiy a developer if it does appear)");
                }
            }
            catch (InvalidUniqueKeyException ex)
            {
                ctx.Client.Logger.LogError(ex, "Failed to generate a unique key.");
                await RespondError("Failed to generate a unique warn key. Please try this command again.");
            }
        }
    }
}
