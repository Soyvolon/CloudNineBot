using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using CloudNine.Discord.Services;

using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity.Extensions;

namespace CloudNine.Discord.Commands.MagicChannel
{
    public class AddMagicChannelCommand : CommandModule
    {
        private MagicChannelService _magic;

        public AddMagicChannelCommand(MagicChannelService magic)
        {
            _magic = magic;
        }

        [Command("magic")]
        [Description("Sets a channel that lets a percentage of people every set time. This will modify the channel so only the selected role" +
            " and proivded additional roles can view the channel.")]
        [RequireUserPermissions(Permissions.ManageChannels)]
        public async Task AddMagicChannelCommandAsync(CommandContext ctx,
            [Description("The channel to set as magic.")]
            DiscordChannel channel,

            [Description("Discord role to assign")]
            DiscordRole role,

            [Description("The percentage of people to let in from the server.")]
            decimal percentage,

            [Description("The delay between each group of people getting let in.")]
            TimeSpan delay,
            
            [Description("Auto remove a member after a set number of messages sent. Set to 0 for no message removal.")]
            int messages = 0)
        {
            var percent = Math.Round(percentage, decimals: 2, MidpointRounding.AwayFromZero);
            if (percent <= 0 || percent > 1)
            {
                await RespondError("Percentage needs to be between 0.01 and 1");
                return;
            }

            var interact = ctx.Client.GetInteractivity();

            await ctx.RespondAsync("Please mention roles and users to ignore: ");

            var ires = await interact.WaitForMessageAsync(x => x.Author.Id == ctx.Member.Id);

            if(ires.TimedOut)
            {
                await RespondError("Timed out.");
                return;
            }

            var roleList = ires.Result.MentionedRoles;
            var memberList = ires.Result.MentionedUsers;

            HashSet<ulong> roles = new();
            foreach (var r in roleList)
                roles.Add(r.Id);

            HashSet<ulong> users = new();
            foreach (var m in memberList)
                roles.Add(m.Id);

            await ctx.TriggerTypingAsync();

            try
            {
                for(int i = 0; i < channel.PermissionOverwrites.Count; i++)
                {
                    var ow = channel.PermissionOverwrites[i];
                    if(ow.Type == OverwriteType.Role)
                    {
                        ulong id;
                        try
                        {
                            id = (await ow.GetRoleAsync()).Id;
                        }
                        catch
                        {
                            try
                            {
                                id = (await ow.GetMemberAsync()).Id;
                            }
                            catch
                            {
                                continue;
                            }
                        }

                        if(!roles.Contains(id) && !users.Contains(id))
                        {
                            await ow.UpdateAsync(deny: Permissions.All, allow: Permissions.None);
                        }

                        await Task.Delay(TimeSpan.FromSeconds(0.5));
                    }
                }

                await channel.AddOverwriteAsync(role, Permissions.AccessChannels | Permissions.SendMessages | Permissions.ReadMessageHistory, reason: "Magic.");
            } 
            catch (Exception ex)
            {
                await RespondError($"Modifying the channel failed, aborting setup.\n\n{ex.Message}\n```{ex.StackTrace}```");
                return;
            }

            await _magic.UpdateOrAddChanelAsync(channel.Id, () => new()
            {
                IgnoreUsersToAdd = users,
                IgnoreRolesToAdd = roles,
                Interval = delay,
                Percentage = percent,
                RemoveAfterMessages = messages,
                Role = role.Id,
            });

            await Respond("Setup complete!");
        }
    }
}
