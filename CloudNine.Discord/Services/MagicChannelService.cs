using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using CloudNine.Core.Database;
using CloudNine.Core.Extensions;
using CloudNine.Core.MagicChannel;

using DSharpPlus;
using DSharpPlus.Entities;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace CloudNine.Discord.Services
{
    public class MagicChannelService
    {
        private readonly IServiceProvider _services;
        private readonly DiscordShardedClient _client;
        private readonly DiscordRestClient _rest;
        private readonly ILogger _logger;

        private ConcurrentDictionary<ulong, Timer> ActiveChannels { get; init; }
        private ConcurrentDictionary<ulong, ConcurrentDictionary<ulong, (int, int, ulong)>> MessageCounts { get; init; }

        public MagicChannelService(IServiceProvider services, DiscordShardedClient client,
            DiscordRestClient rest)
        {
            _services = services;
            _client = client;
            _rest = rest;
            _logger = _client.Logger;

            ActiveChannels = new();
            MessageCounts = new();
        }

        public async Task InitalizeAsync()
        {
            _client.MessageCreated += Client_MessageReceived;

            var db = _services.GetRequiredService<CloudNineDatabaseModel>();

            int delay = 0;
            await db.MagicChannels.AsNoTracking().ForEachAsync(x =>
            {
                AddChannelAsync(x, delay++);
            });
        }

        public async Task UpdateOrAddChanelAsync(ulong channelId, Func<MagicChannelUpdater> update)
        {
            var db = _services.GetRequiredService<CloudNineDatabaseModel>();
            var chan = await db.FindAsync<MagicChannelData>(channelId);

            bool added = false;
            if (chan is null)
            {
                chan = new()
                {
                    ChannelId = channelId
                };

                await db.AddAsync(chan);
                await db.SaveChangesAsync();
                added = true;
            }

            var data = update.Invoke();

            chan.UsersToIgnore.UnionWith(data.IgnoreUsersToAdd);
            chan.UsersToIgnore.ExceptWith(data.IgnoreUsersToRemove);

            chan.RolesToIgnore.UnionWith(data.IgnoreRolesToAdd);
            chan.RolesToIgnore.ExceptWith(data.IgnoreRolesToRemove);

            if (data.Interval is not null)
                chan.Interval = data.Interval.Value;

            if (data.Percentage is not null)
                chan.MemberPercent = data.Percentage.Value;

            if (data.RemoveAfterMessages is not null)
                chan.RemoveAfterMessages = data.RemoveAfterMessages.Value;

            if (data.Role is not null)
                chan.RoleToAssign = data.Role.Value;

            db.Update(chan);
            await db.SaveChangesAsync();

            if(added)
                AddChannelAsync(chan);
        }

        public async Task RemoveChannelAsync(ulong channelId)
        {
            var db = _services.GetRequiredService<CloudNineDatabaseModel>();
            var chan = await db.FindAsync<MagicChannelData>(channelId);

            if(chan is not null)
            {
                _ = ActiveChannels.TryRemove(chan.ChannelId, out _);
                _ = MessageCounts.TryRemove(chan.ChannelId, out _);
            }

            db.Remove(chan);
            await db.SaveChangesAsync();
        }

        private void AddChannelAsync(MagicChannelData channel, int initdelay = 0)
        {
            ActiveChannels[channel.ChannelId] = new Timer(async (x) => await PopulateChannelAsync(channel.ChannelId), null, TimeSpan.FromMinutes(initdelay), channel.Interval);
            MessageCounts[channel.ChannelId] = new();
        }

        private async Task PopulateChannelAsync(ulong cid, TimeSpan? interval = null)
        {
            var db = _services.GetRequiredService<CloudNineDatabaseModel>();
            var channel = await db.FindAsync<MagicChannelData>(cid);

            if (interval is null)
            {
                var rundiff = DateTime.UtcNow - channel.LastRun;
                var saftey = channel.Interval - TimeSpan.FromSeconds(30);
                if (rundiff < saftey)
                {
                    ActiveChannels[cid] = new Timer(async (x) => await PopulateChannelAsync(cid, channel.Interval), null, rundiff, Timeout.InfiniteTimeSpan);
                    return;
                }
            }
            else
            {
                ActiveChannels[cid].Change(interval.Value, interval.Value);
                return;
            }

            var chan = await _rest.GetChannelAsync(channel.ChannelId);

            if (chan.GuildId is null) return;

            DiscordGuild? guild = null;
            foreach (var shard in _client.ShardClients.Values)
                if (!shard.Guilds.TryGetValue(chan.GuildId.Value, out guild)) return;

            var members = await guild.GetAllMembersAsync();

            HashSet<DiscordMember> valid = new();
            HashSet<DiscordMember> toRemove = new();

            List<Task> tasks = new();
            foreach(var mem in members)
            {
                tasks.Add(Task.Run(() =>
                {
                    try
                    {
                        if (channel.UsersToIgnore.Contains(mem.Id))
                            return;
                        else if (mem.Roles.Any(x => channel.RolesToIgnore.Contains(x.Id)))
                            return;
                        else if (mem.Roles.Any(x => channel.RoleToAssign.Equals(x.Id)))
                            toRemove.Add(mem);
                        else
                            valid.Add(mem);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Magic member add failed.");
                    }
                }));
            }

            await Task.WhenAll(tasks);

            try
            {
                var role = guild.GetRole(channel.RoleToAssign);
                foreach (var mem in toRemove)
                {
                    _ = Task.Run(async () =>
                    {
                        if (mem is null) return;

                        await mem.RevokeRoleAsync(role, "Magic.");

                        if (MessageCounts.TryGetValue(channel.ChannelId, out var mcount))
                            _ = mcount.TryRemove(mem.Id, out _);
                    });
                }

                HashSet<DiscordMember> selection = new();

                do
                {
                    if (valid.Count <= 0) break;

                    selection.Add(valid.Random());
                } while (selection.Count < 0
                || ((decimal)selection.Count / valid.Count) < channel.MemberPercent);

                foreach (var mem in selection)
                {
                    if (mem is null) continue;

                    _ = Task.Run(async () =>
                    {
                        await mem.GrantRoleAsync(role, "Magic.");

                        MessageCounts[channel.ChannelId][mem.Id] = (0, channel.RemoveAfterMessages, channel.RoleToAssign);
                    });
                }

                channel.LastRun = DateTime.UtcNow;

                db.Update(channel);
                await db.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to get role from guild.");
            }
        }

        private Task Client_MessageReceived(DiscordClient sender, DSharpPlus.EventArgs.MessageCreateEventArgs e)
        {
            _ = Task.Run(async () =>
            {
                if (ActiveChannels.ContainsKey(e.Channel.Id))
                {
                    if(MessageCounts.TryGetValue(e.Channel.Id, out var members))
                    {
                        if (members.TryRemove(e.Author.Id, out var data))
                        {
                            var c = data.Item1 + 1;

                            if (c >= data.Item2)
                            {
                                try
                                {
                                    await _rest.RemoveGuildMemberRoleAsync(e.Guild.Id, e.Author.Id, data.Item3, "Magic.");
                                }
                                catch { /* no role found or something */ }
                            }

                            MessageCounts[e.Channel.Id][e.Author.Id] = (c, data.Item2, data.Item3);
                        }
                    }
                }
            });

            return Task.CompletedTask;
        }
    }
}
