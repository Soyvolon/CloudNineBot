using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CloudNine.Core.Moderation
{
    public class ModCore
    {
        private readonly Random _rand;

        [Key]
        public ulong GuildId { get; set; }

        [NotMapped]
        public ConcurrentDictionary<ulong, SortedDictionary<DateTime, Warn>> Warns
        {
            get
            {
                if (_warns is null)
                    return InitWarns();

                return _warns;
            }
        }        
        private ConcurrentDictionary<ulong, SortedDictionary<DateTime, Warn>>? _warns;

        public HashSet<Warn> WarnSet { get; set; }

        [NotMapped]
        public HashSet<string> Keys
        {
            get
            {
                if (_keys is null)
                    return InitKeys();

                return _keys;
            }
        }
        private HashSet<string>? _keys;

        public ModCore() : this(0, new()) { }

        public ModCore(ulong guildId) : this(guildId, new()) { }

        public ModCore(ulong guildId, HashSet<Warn> warnSet)
        {
            GuildId = guildId;
            WarnSet = warnSet;
            _rand = new Random();
        }

        public async Task<bool> AddWarn(ulong userid, string message, ulong savedby)
        {
            return AddWarn(new(await GetGuildUniqueKey(), userid, message, savedby));
        }

        public bool AddWarn(Warn w)
        {
            var res = WarnSet.Add(w);
            if (res)
            {
                if (_warns is null)
                {
                    _warns = InitWarns();
                }
                else
                {
                    if (_warns.ContainsKey(w.UserId))
                        _warns[w.UserId].Add(w.CreatedOn, w);
                    else
                        _warns[w.UserId] = new() { { w.CreatedOn, w } };
                }

                if (_keys is null)
                {
                    _keys = InitKeys();
                }
                else
                {
                    _keys.Add(w.Key);
                }
            }

            return res;
        }

        public bool RemoveWarn(string key)
        {
            var res = WarnSet.Where(x => x.Key == key).FirstOrDefault();

            if (res != default)
                return RemoveWarn(res);

            return false;
        }

        public bool RemoveWarn(Warn w)
        {
            var res = WarnSet.Remove(w);
            _warns?.TryRemove(w.UserId, out _);
            _keys?.Remove(w.Key);
            return res;
        }

        private ConcurrentDictionary<ulong, SortedDictionary<DateTime, Warn>> InitWarns()
        {
            _warns = new();
            foreach(var w in WarnSet)
            {
                if(_warns.ContainsKey(w.UserId))
                {
                    _warns[w.UserId].Add(w.CreatedOn, w);
                }
                else
                {
                    _warns[w.UserId] = new SortedDictionary<DateTime, Warn>() { { w.CreatedOn, w } };
                }
            }

            return _warns;
        }

        private HashSet<string> InitKeys()
        {
            _keys = new();

            foreach(var w in WarnSet)
                _keys.Add(w.Key);

            return _keys;
        }

        public Task<string> GetGuildUniqueKey()
        {
            int exitCounter = 0;
            while (exitCounter++ < 20) // attempt this generation 20 times.
            {
                string output = "";
                lock (_rand)
                {
                    int keylength = _rand.Next(10, 16);

                    for(int i = 0; i < keylength; i++)
                    {
                        var num = _rand.Next(33, 127);
                        if (num == 96) num = 97; // dont use `, it messes with formatting.
                        if (num == 34) num = 35; // dont use ", breaks SQL

                        output += (char)num;
                    }
                }

                if (!Keys.Contains(output))
                    return Task.FromResult(output);
            }

            throw new InvalidUniqueKeyException("Failed to generate a unique key.");
        }

        public class InvalidUniqueKeyException : Exception
        {
            public InvalidUniqueKeyException() : base() { }
            public InvalidUniqueKeyException(string? message) : base(message) { }
        }
    }
}
