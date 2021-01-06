using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Timers;

using CloudNine.Core.Birthdays;
using CloudNine.Core.Configuration;
using CloudNine.Core.Database;

using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.Exceptions;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace CloudNine.Discord.Utilities
{
    public class BirthdayManager : IDisposable
    {
        public readonly int TriggerBdayAt = 12;
        public bool DebugTrigger { get; set; } = false;

        private Timer bdayChecker;
        private DateTime lastDay;
        private bool disposedValue;
        private const int elapsedCounter = 1;
        private readonly ServiceProvider _services;
        private readonly ILogger _logger;

        public BirthdayManager(int trigger_at, ServiceProvider services)
        {
            TriggerBdayAt = trigger_at;

            bdayChecker = new Timer(5000);
            bdayChecker.Elapsed += BdayChecker_Elapsed;
            bdayChecker.Start();
            lastDay = DateTime.UtcNow.AddDays(-1);
            this._services = services;
            _logger = services.GetService<ILogger<BirthdayManager>>();
        }

        private async void BdayChecker_Elapsed(object sender, ElapsedEventArgs e)
        {
            try
            {
                if (DebugTrigger || ((DateTime.UtcNow.Date - lastDay.Date).TotalDays >= 1 && DateTime.UtcNow.Hour >= TriggerBdayAt))
                {
                    lastDay = DateTime.UtcNow;
                    DebugTrigger = false;

                    _logger.Log(LogLevel.Information, new EventId(elapsedCounter, "Bday Timer"), "Birthday Timer Elapsed", null);

                    var _database = _services.GetRequiredService<CloudNineDatabaseModel>();
                    foreach (var server in _database.ServerConfigurations)
                    {
                        try
                        {
                            if (server.BirthdayConfiguration.BirthdayChannel is null) continue;
                            var shard = await DiscordBot.Bot.GetClientForGuildId(server.Id);
                            if (shard is null) continue;

                            server.BirthdayConfiguration.ResetComparer();
                            server.BirthdayConfiguration.TriggerResort();

                            var toLockout = server.BirthdayConfiguration.GetNextBirthdaysToLockout();

                            if (toLockout is null) continue;

                            var guild = await shard.GetGuildAsync(server.Id);

                            var channels = await guild.GetChannelsAsync().ConfigureAwait(false);

                            var chan = channels.FirstOrDefault(x => x.Id == server.BirthdayConfiguration.BirthdayChannel);

                            if (chan == default) continue;

                            HashSet<ulong> usersToRemove = new HashSet<ulong>();

                            foreach (var user in toLockout.Values)
                            {
                                foreach (var id in user)
                                {
                                    try
                                    {
                                        var member = await guild.GetMemberAsync(id).ConfigureAwait(false);

                                        var perms = chan.PermissionsFor(member);

                                        if (((uint)perms & (uint)Permissions.AccessChannels) == (uint)Permissions.AccessChannels && perms != Permissions.All)
                                        {
                                            try
                                            {
                                                await chan.AddOverwriteAsync(member, Permissions.None, Permissions.AccessChannels, "Auto Brithday Lockout").ConfigureAwait(false);

                                                var dm = await member.CreateDmChannelAsync();
                                                await dm.SendMessageAsync($"Looks like your birthday is coming up soon! You are now locked out of {chan.Name} so people can plot your gifts in secret!");
                                                //await Program.Bot.Rest.EditChannelPermissionsAsync((ulong)server.Value.BirthdayChannel, id, Permissions.None, Permissions.AccessChannels, "member", "Auto Birthday Lockout").ConfigureAwait(false);
                                                //await Task.Delay(TimeSpan.FromSeconds(1)).ConfigureAwait(false);
                                            }
                                            catch (UnauthorizedException ex)
                                            {
                                                throw ex;
                                                continue;
                                            }
                                        }
                                    }
                                    catch (NotFoundException)
                                    {
                                        _logger.LogWarning("Failed to get a user, removing them from the birthday list.");
                                        usersToRemove.Add(id);
                                        continue;
                                    }
                                }
                            }

                            var currentBday = server.BirthdayConfiguration.GetBirthdaysOnToday();

                            string msg = "";

                            foreach (var bday in currentBday)
                            {
                                try
                                {
                                    var member = await guild.GetMemberAsync(bday).ConfigureAwait(false);

                                    var perms = chan.PermissionsFor(member);

                                    if (((uint)perms & (uint)Permissions.AccessChannels) != (uint)Permissions.AccessChannels)
                                    {
                                        try
                                        {
                                            await chan.AddOverwriteAsync(member, Permissions.AccessChannels, Permissions.None, "Auto Birtday Access").ConfigureAwait(false);
                                            //await Program.Bot.Rest.EditChannelPermissionsAsync((ulong)server.Value.BirthdayChannel, bday, Permissions.AccessChannels, Permissions.None, "member", "Auto Birthday Access").ConfigureAwait(false);
                                            //await Task.Delay(TimeSpan.FromSeconds(1)).ConfigureAwait(false);
                                        }
                                        catch (UnauthorizedException ex)
                                        {
                                            throw ex;
                                            continue;
                                        }
                                    }
                                    msg += $"Happy Birthday {member.Mention}\n";
                                }
                                catch (NotFoundException)
                                {
                                    _logger.LogWarning("Failed to get a user, removing them from the birthday list.");
                                    usersToRemove.Add(bday);
                                    continue;
                                }
                            }

                            if (!(msg is null) && msg != "")
                                await chan.SendMessageAsync(msg[..(msg.Length - 1)]).ConfigureAwait(false);

                            var allOthers = server.BirthdayConfiguration.GetNonLockoutNonBirthdayUsers();

                            foreach (var other in allOthers)
                            {
                                try
                                {
                                    var member = await guild.GetMemberAsync(other).ConfigureAwait(false);

                                    var perms = chan.PermissionsFor(member);

                                    if (((uint)perms & (uint)Permissions.AccessChannels) != (uint)Permissions.AccessChannels)
                                    {
                                        try
                                        {
                                            await chan.AddOverwriteAsync(member, Permissions.AccessChannels, Permissions.None, "Auto Birtday Access").ConfigureAwait(false);
                                            //await Program.Bot.Rest.EditChannelPermissionsAsync((ulong)server.Value.BirthdayChannel, other, Permissions.AccessChannels, Permissions.None, "member", "Auto Birthday Access").ConfigureAwait(false);
                                            //await Task.Delay(TimeSpan.FromSeconds(1)).ConfigureAwait(false);
                                        }
                                        catch (UnauthorizedException ex)
                                        {
                                            throw ex;
                                            continue;
                                        }
                                    }
                                }
                                catch (NotFoundException)
                                {
                                    _logger.LogWarning("Failed to get a user, removing them from the birthday list.");
                                    usersToRemove.Add(other);
                                    continue;
                                }
                            }

                            _database.Update(server);

                            foreach (var u in usersToRemove)
                            {
                                server.BirthdayConfiguration.RemoveBirthday(u);
                            }

                            await _database.SaveChangesAsync();

                            await UpdateChannelDescription(chan.Id, server.BirthdayConfiguration, server.Id).ConfigureAwait(false);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "Failed to execute birthday update.");
                            continue;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Something went wrong in the Birthday Checker");
            }
        }

        private async Task UpdateChannelDescription(ulong c, BirthdayServerConfiguration server, ulong id)
        {
            var channelTopic = "Next Birthdays: ";

            var nextBdays = server.GetNextBirthdaysToLockout();

            if (nextBdays is null || nextBdays.Count == 0) channelTopic += $"No Birthdays?! :sob:  ";
            else if (nextBdays.Count >= 1)
            {
                var item = nextBdays.ElementAt(0);

                foreach (var bday in item.Value)
                {
                    channelTopic += $"<@{bday}> {item.Key:dd} {item.Key:MMMM}, ";
                }
            }

            if (!(nextBdays is null) && nextBdays.Count >= 2)
            {
                var item = nextBdays.ElementAt(1);

                foreach (var bday in item.Value)
                {
                    channelTopic += $"<@{bday}> {item.Key:dd} {item.Key:MMMM}, ";
                }
            }

            channelTopic = channelTopic[..(channelTopic.Length - 2)];

            if (channelTopic.Length > 1024)
            {
                channelTopic = channelTopic[..1024];
            }

            if (DiscordBot.IsDebug)
            { // Use this to get around the channel update rate limits.
                await DiscordBot.Bot.Rest.CreateMessageAsync(755610860680118283, channelTopic).ConfigureAwait(false);
            }
            else
            {
                await DiscordBot.Bot.Rest.ModifyChannelAsync(c, x => x.Topic = channelTopic).ConfigureAwait(false);
            }
        }

        private void UpdateComparisonDay()
        {
            var _database = _services.GetRequiredService<CloudNineDatabaseModel>();
            foreach (var val in _database.ServerConfigurations.AsNoTracking())
                val.BirthdayConfiguration.TriggerResort();
        }

        public async Task<bool> ForceChannelUpdate(DiscordGuild g)
        {
            var _database = _services.GetRequiredService<CloudNineDatabaseModel>();
            var config = await _database.FindAsync<DiscordGuildConfiguration>(g.Id);
            if (!(config is null))
            {
                if (config.BirthdayConfiguration.BirthdayChannel is null) return false;

                await UpdateChannelDescription((ulong)config.BirthdayConfiguration.BirthdayChannel, config.BirthdayConfiguration, g.Id).ConfigureAwait(false);

                return true;
            }

            return false;
        }

        public void UpdateBirthday(ulong server, ulong user, DateTime date)
        {
            var _database = _services.GetRequiredService<CloudNineDatabaseModel>();
            var cfg = _database.Find<DiscordGuildConfiguration>(server);
            if (!(cfg is null))
            {
                _database.Update(cfg);
                cfg.BirthdayConfiguration.UpdateBirthday(user, date);
            }
            else
            {
                cfg = new DiscordGuildConfiguration()
                {
                    Id = server,
                    Prefix = DiscordBot.Bot.BotConfiguration.Prefix
                };

                _database.Add(cfg);
            }

            _database.SaveChanges();
        }

        public bool RemoveBirthday(ulong server, ulong user)
        {
            var _database = _services.GetRequiredService<CloudNineDatabaseModel>();
            var cfg = _database.Find<DiscordGuildConfiguration>(server);
            if (!(cfg is null))
            {
                if (cfg.BirthdayConfiguration.RemoveBirthday(user))
                {
                    _database.Update(cfg);
                    _database.SaveChanges();

                    return true;
                }
            }

            return false;
        }

        public DateTime? GetBirthday(ulong server, ulong user)
        {
            var _database = _services.GetRequiredService<CloudNineDatabaseModel>();
            var cfg = _database.Find<DiscordGuildConfiguration>(server);
            if (!(cfg is null))
            {
                return cfg.BirthdayConfiguration.GetBirthday(user);
            }

            return null;
        }

        public Dictionary<ulong, List<ulong>> GetAllBirthdaysOnToday()
        {
            var _database = _services.GetRequiredService<CloudNineDatabaseModel>();
            var values = new Dictionary<ulong, List<ulong>>();

            foreach (var pair in _database.ServerConfigurations.AsNoTracking())
            {
                values.Add(pair.Id, pair.BirthdayConfiguration.GetBirthdaysOnToday());
            }

            return values;
        }

        public List<ulong>? GetBrithdaysOnTodayForServer(ulong server)
        {
            var _database = _services.GetRequiredService<CloudNineDatabaseModel>();
            var config = _database.Find<DiscordGuildConfiguration>(server);
            if (!(config is null))
            {
                return config.BirthdayConfiguration.GetBirthdaysOnToday();
            }

            return null;
        }

        public SortedList<DateTime, List<ulong>>? GetAllBirthdaysForServer(ulong server, bool keepCurrentSort)
        {
            var _database = _services.GetRequiredService<CloudNineDatabaseModel>();
            var config = _database.Find<DiscordGuildConfiguration>(server);
            if (!(config is null))
            {
                if (keepCurrentSort) return config.BirthdayConfiguration.SortedBirthdays;

                return new SortedList<DateTime, List<ulong>>(config.BirthdayConfiguration.SortedBirthdays);
            }

            return null;
        }

        public void UpdateBirthdayChannel(ulong server, DiscordChannel channel)
        {
            var _database = _services.GetRequiredService<CloudNineDatabaseModel>();
            var cfg = _database.Find<DiscordGuildConfiguration>(server);
            if (!(cfg is null))
            {
                _database.Update(cfg);
                cfg.BirthdayConfiguration.BirthdayChannel = channel.Id;
            }
            else
            {
                cfg = new DiscordGuildConfiguration()
                {
                    Id = server,
                    BirthdayConfiguration = new BirthdayServerConfiguration()
                    {
                        BirthdayChannel = channel.Id
                    }
                };

                _database.Add(cfg);
            }

            _database.SaveChanges();
        }

        public void UpdateBirthdayRole(ulong server, DiscordRole role)
        {
            var _database = _services.GetRequiredService<CloudNineDatabaseModel>();
            var cfg = _database.Find<DiscordGuildConfiguration>(server);
            if (!(cfg is null))
            {
                _database.Update(cfg);
                cfg.BirthdayConfiguration.BirthdayRole = role.Id;
            }
            else
            {
                cfg = new DiscordGuildConfiguration()
                {
                    Id = server,
                    BirthdayConfiguration = new BirthdayServerConfiguration()
                    {
                        BirthdayRole = role.Id
                    }
                };

                _database.Add(cfg);
            }

            _database.SaveChanges();
        }



        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects)
                }

                // TODO: free unmanaged resources (unmanaged objects) and override finalizer
                // TODO: set large fields to null
                bdayChecker = null;
                lastDay = DateTime.MinValue;
                disposedValue = true;
            }
        }

        // // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
        // ~BirthdayManager()
        // {
        //     // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        //     Dispose(disposing: false);
        // }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
