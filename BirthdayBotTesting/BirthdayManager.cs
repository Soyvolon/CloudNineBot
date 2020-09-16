using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Timers;

using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.Exceptions;

using Microsoft.Extensions.Logging;

using Newtonsoft.Json;
using DSharpPlus.CommandsNext;

namespace BirthdayBotTesting
{
    public class BirthdayManager
    {
        public bool DebugTrigger { get; set; } = false;

        private const string ConfigDir = "ServerConfigs";

        private ConcurrentDictionary<ulong, ServerConfiguration> ServerConfigs;

        private Timer bdayChecker;
        private DateTime lastDay;

        private const int elapsedCounter = 1;
        public readonly int TriggerBdayAt = 12;

        public BirthdayManager(int trigger_at)
        {
            if (!Directory.Exists(ConfigDir))
                Directory.CreateDirectory(ConfigDir);

            ServerConfigs = new ConcurrentDictionary<ulong, ServerConfiguration>();

            PopulateInitialBirthdayWatchlist();

            TriggerBdayAt = trigger_at;

            bdayChecker = new Timer(1000);
            bdayChecker.Elapsed += BdayChecker_Elapsed;
            bdayChecker.Start();
            lastDay = DateTime.UtcNow.AddDays(-1);
        }

        private async void BdayChecker_Elapsed(object sender, ElapsedEventArgs e)
        {
            try
            {
                if (DebugTrigger || ((DateTime.UtcNow.Date - lastDay.Date).TotalDays >= 1 && DateTime.UtcNow.Hour >= TriggerBdayAt))
                {
                    Program.Bot.Client.Logger.Log(LogLevel.Information, new EventId(elapsedCounter, "Bday Timer"), "Birthday Timer Elapsed", null);

                    lastDay = DateTime.UtcNow;
                    DebugTrigger = false;

                    foreach (var server in ServerConfigs)
                    {
                        try
                        {
                            if (server.Value.BirthdayChannel is null) continue;

                            server.Value.ResetComparer();
                            server.Value.TriggerResort();

                            var toLockout = server.Value.GetNextBirthdaysToLockout();

                            if (toLockout is null) continue;

                            var shard = Program.Bot.Client;

                            var guild = await shard.GetGuildAsync(server.Key);

                            var channels = await guild.GetChannelsAsync().ConfigureAwait(false);

                            var chan = channels.First(x => x.Id == server.Value.BirthdayChannel);

                            foreach (var user in toLockout.Values)
                            {
                                foreach (var id in user)
                                {
                                    var member = await guild.GetMemberAsync(id).ConfigureAwait(false);

                                    var perms = chan.PermissionsFor(member);

                                    if (((uint)perms & (uint)Permissions.AccessChannels) == (uint)Permissions.AccessChannels && perms != Permissions.All)
                                    {
                                        try
                                        {
                                            await chan.AddOverwriteAsync(member, Permissions.None, Permissions.AccessChannels, "Auto Brithday Lockout").ConfigureAwait(false);
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
                            }

                            var currentBday = server.Value.GetBirthdaysOnToday();

                            string msg = "";

                            foreach (var bday in currentBday)
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

                            if (!(msg is null) && msg != "")
                                await chan.SendMessageAsync(msg[..(msg.Length - 1)]).ConfigureAwait(false);

                            var allOthers = server.Value.GetNonLockoutNonBirthdayUsers();

                            foreach (var other in allOthers)
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

                            await UpdateChannelDescription(chan.Id, server.Value, server.Key).ConfigureAwait(false);
                        }
                        catch
                        {
                            continue;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Program.Bot.Client.Logger.LogError(ex, "Something went wrong in the Birthday Checker");
            }
        }

        private async Task UpdateChannelDescription(ulong c, ServerConfiguration server, ulong id)
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

            if (Program.IsDebug)
            { // Use this to get around the channel update rate limits.
                await Program.Bot.Rest.CreateMessageAsync(755610860680118283, channelTopic, false, null, null).ConfigureAwait(false);
            }
            else
            {
                await Program.Bot.Rest.ModifyChannelAsync(c, x => x.Topic = channelTopic).ConfigureAwait(false);
            }
        }

        private void UpdateComparisonDay()
        {
            foreach (var val in ServerConfigs.Values)
                val.TriggerResort();
        }

        private void PopulateInitialBirthdayWatchlist()
        {
            var files = Directory.GetFiles(ConfigDir, "*");

            foreach (var file in files)
            {
                if (ulong.TryParse(Path.GetFileNameWithoutExtension(file), out ulong result))
                {
                    var json = File.ReadAllText(file);

                    var config = JsonConvert.DeserializeObject<ServerConfiguration>(json);

                    if(ServerConfigs.TryAdd(result, config))
                    {
                        config.BuildInitalSortList();
                    }
                }
            }
        }

        private void SaveConfigFile(ulong server)
        {
            if (ServerConfigs.TryGetValue(server, out ServerConfiguration? value))
            {
                File.WriteAllText(Path.Combine(new string[] { ConfigDir, server.ToString() + ".json" }),
                    JsonConvert.SerializeObject(value, Formatting.Indented));
            }
        }

        public async Task<bool> ForceChannelUpdate(DiscordGuild g)
        {
            if(ServerConfigs.TryGetValue(g.Id, out ServerConfiguration? config))
            {
                if (config.BirthdayChannel is null) return false;

                await UpdateChannelDescription((ulong)config.BirthdayChannel, config, g.Id).ConfigureAwait(false);

                return true;
            }

            return false;
        }

        public void SaveAllConfigurations()
        {
            foreach (var server in ServerConfigs.Keys)
                SaveConfigFile(server);
        }

        public void GenerateNewServerConfiguration(ulong server)
        {
            GenerateNewServerConfiguartionWithValue(server, null, null);
        }

        public void GenerateNewServerConfiguartionWithValue(ulong server, ulong? user, DateTime? date)
        {
            var data = new ConcurrentDictionary<ulong, DateTime>();

            var config = new ServerConfiguration(data);

            if (!(date is null || user is null))
            {
                config.UpdateBirthday((ulong)user, (DateTime)date);
            }

            ServerConfigs.TryAdd(server, config);
        }

        public void UpdateBirthday(ulong server, ulong user, DateTime date)
        {
            if (ServerConfigs.TryGetValue(server, out ServerConfiguration? cfg))
            {
                cfg.UpdateBirthday(user, date);
            }
            else
            {
                GenerateNewServerConfiguartionWithValue(server, user, date);
            }

            SaveConfigFile(server);
        }

        public bool RemoveBirthday(ulong server, ulong user)
        {
            if(ServerConfigs.TryGetValue(server, out ServerConfiguration? cfg))
            {
                if (cfg.RemoveBirthday(user))
                {
                    SaveConfigFile(server);

                    return true;
                }
            }

            return false;
        }

        public DateTime? GetBirthday(ulong server, ulong user)
        {
            if (ServerConfigs.TryGetValue(server, out ServerConfiguration? cfg))
            {
                return cfg.GetBirthday(user);
            }

            return null;
        }

        public Dictionary<ulong, List<ulong>> GetAllBirthdaysOnToday()
        {
            var values = new Dictionary<ulong, List<ulong>>();

            foreach (var pair in ServerConfigs)
            {
                values.Add(pair.Key, pair.Value.GetBirthdaysOnToday());
            }

            return values;
        }

        public List<ulong>? GetBrithdaysOnTodayForServer(ulong server)
        {
            if (ServerConfigs.TryGetValue(server, out ServerConfiguration? config))
            {
                return config.GetBirthdaysOnToday();
            }

            return null;
        }

        public SortedList<DateTime, List<ulong>>? GetAllBirthdaysForServer(ulong server, bool keepCurrentSort)
        {
            if(ServerConfigs.TryGetValue(server, out ServerConfiguration? config))
            {
                if (keepCurrentSort) return config.SortedBirthdays;

                return new SortedList<DateTime, List<ulong>>(config.SortedBirthdays);
            }

            return null;
        }

        public void UpdateBirthdayChannel(ulong server, DiscordChannel channel)
        {
            if (ServerConfigs.TryGetValue(server, out ServerConfiguration? cfg))
            {
                cfg.BirthdayChannel = channel.Id;
            }
            else
            {
                cfg = new ServerConfiguration(new ConcurrentDictionary<ulong, DateTime>())
                {
                    BirthdayChannel = channel.Id
                };
                ServerConfigs.TryAdd(server, cfg);
            }

            SaveConfigFile(server);
        }

        public void UpdateBirthdayRole(ulong server, DiscordRole role)
        {
            if (ServerConfigs.TryGetValue(server, out ServerConfiguration? cfg))
            {
                cfg.BirthdayRole = role.Id;
            }
            else
            {
                cfg = new ServerConfiguration(new ConcurrentDictionary<ulong, DateTime>())
                {
                    BirthdayRole = role.Id
                };
                ServerConfigs.TryAdd(server, cfg);
            }

            SaveConfigFile(server);
        }

        private class ServerConfiguration
        {
            [JsonProperty("birthdays")]
            public ConcurrentDictionary<ulong, DateTime> BirthdayDictionary { get; private set; }

            [JsonProperty("bday_channel")]
            public ulong? BirthdayChannel { get; set; }
            [JsonProperty("bday_role")]
            public ulong? BirthdayRole { get; set; }

            [JsonIgnore]
            public SortedList<DateTime, List<ulong>> SortedBirthdays { get; private set; }
            [JsonIgnore]
            private BirthdayDateComparer Comparer;

            public ServerConfiguration(ConcurrentDictionary<ulong, DateTime> data)
            {
                Comparer = new BirthdayDateComparer(DateTime.UtcNow);
                BirthdayDictionary = data;
                SortedBirthdays = new SortedList<DateTime, List<ulong>>(Comparer);
            }

            public void TickComparer()
            {
                Comparer.CurrentDay = Comparer.CurrentDay.AddDays(1);
            }

            public void ResetComparer()
            {
                Comparer = new BirthdayDateComparer(DateTime.UtcNow);
            }

            public void BuildInitalSortList()
            {
                foreach (var bday in BirthdayDictionary)
                {
                    if (!SortedBirthdays.TryAdd(bday.Value, new List<ulong>() { bday.Key }))
                        SortedBirthdays[bday.Value].Add(bday.Key);
                }
            }

            public void TriggerResort()
            {
                SortedList<DateTime, List<ulong>> newData = new SortedList<DateTime, List<ulong>>(Comparer);

                foreach (var item in SortedBirthdays)
                    newData.Add(item.Key, item.Value);

                SortedBirthdays = newData;
            }

            public void UpdateBirthday(ulong user, DateTime birthday)
            {
                if(BirthdayDictionary.ContainsKey(user))
                    RemoveFromSortedList(user, BirthdayDictionary[user]);
                
                BirthdayDictionary[user] = birthday;

                if (!SortedBirthdays.TryAdd(birthday, new List<ulong>() { user }))
                    SortedBirthdays[birthday].Add(user);
            }

            public bool RemoveBirthday(ulong user)
            {
                if(BirthdayDictionary.TryRemove(user, out DateTime date))
                {
                    return RemoveFromSortedList(user, date);
                }

                return false;
            }

            private bool RemoveFromSortedList(ulong user, DateTime date)
            {
                if (SortedBirthdays[date].Remove(user))
                {
                    if (SortedBirthdays[date].Count <= 0)
                        SortedBirthdays.Remove(date);

                    return true;
                }

                return false;
            }

            public DateTime? GetBirthday(ulong user)
            {
                if (BirthdayDictionary.TryGetValue(user, out DateTime value))
                    return value;

                return null;
            }

            public List<ulong> GetBirthdaysOnToday()
            {
                var res = new List<ulong>(SortedBirthdays.FirstOrDefault(
                        x => x.Key.Date.Month == Comparer.CurrentDay.Date.Month &&
                        x.Key.Date.Day == Comparer.CurrentDay.Date.Day
                    ).Value ?? new List<ulong>());

                return res ?? new List<ulong>();
            }

            public SortedList<DateTime, List<ulong>> GetNextBirthdaysToLockout()
            {
                var res = new SortedList<DateTime, List<ulong>>(Comparer);

                var c = 0;
                foreach(var val in SortedBirthdays)
                {
                    if (c >= 3) break;

                    if (c == 0 && (val.Key.Month != Comparer.CurrentDay.Month || val.Key.Day != Comparer.CurrentDay.Day))
                    {
                        c += 2;
                        res.Add(val.Key, val.Value);
                        continue;
                    }

                    if (c != 0)
                        res.Add(val.Key, val.Value);

                    c++;
                }

                return res;
            }

            public List<ulong>? GetBirthdaysWithinLockout_old()
            {
                var res = new List<ulong>();

                foreach (var pair in BirthdayDictionary)
                {
                    var startDate = new DateTime(DateTime.UtcNow.Year, pair.Value.Month, pair.Value.Day) - TimeSpan.FromDays(30);
                    var compareDate = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, DateTime.UtcNow.Day);

                    var diff = compareDate - startDate;

                    if (diff.Days < 30)
                    {
                        res.Add(pair.Key);
                    }
                }

                return res;
            }

            public IEnumerable<ulong> GetNonLockoutNonBirthdayUsers()
            {
                var res = new List<ulong>();
                var lockout = GetNextBirthdaysToLockout().Values;
                var bdays = GetBirthdaysOnToday();

                if (!(lockout is null))
                    foreach (var v in lockout)
                        res.AddRange(v);

                if (!(bdays is null))
                    res.AddRange(bdays);

                return BirthdayDictionary.Keys.ToList().Where(x => !res.Contains(x));
            }

        }
    }
}
