using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Newtonsoft.Json;

namespace CloudNine.Core.Birthdays
{
    public class BirthdayServerConfiguration
    {
        [JsonProperty("birthdays")]
        public ConcurrentDictionary<ulong, DateTime> BirthdayDictionary { get; private set; }

        [JsonProperty("bday_channel")]
        public ulong? BirthdayChannel { get; set; }
        [JsonProperty("bday_role")]
        public ulong? BirthdayRole { get; set; }

        [JsonIgnore]
        public SortedList<DateTime, List<ulong>> SortedBirthdays { get
            {
                if (!initialized) Initalize();
                return _sortedBdays;
            }
        }

        [JsonIgnore]
        private SortedList<DateTime, List<ulong>> _sortedBdays;


        [JsonIgnore]
        private BirthdayDateComparer Comparer;
        [JsonIgnore]
        private bool initialized = false;

        public BirthdayServerConfiguration()
        {
            Comparer = new BirthdayDateComparer(DateTime.UtcNow);
            BirthdayDictionary = new ConcurrentDictionary<ulong, DateTime>();
            _sortedBdays = new SortedList<DateTime, List<ulong>>(Comparer);
        }

        public void TickComparer()
        {
            Comparer.CurrentDay = Comparer.CurrentDay.AddDays(1);
        }

        public void ResetComparer()
        {
            Comparer = new BirthdayDateComparer(DateTime.UtcNow);
        }

        public void Initalize()
        {
            foreach (var bday in BirthdayDictionary)
            {
                if (!_sortedBdays.TryAdd(bday.Value, new List<ulong>() { bday.Key }))
                    _sortedBdays[bday.Value].Add(bday.Key);
            }

            initialized = true;
        }

        public void TriggerResort()
        {
            if (!initialized) Initalize();

            SortedList<DateTime, List<ulong>> newData = new SortedList<DateTime, List<ulong>>(Comparer);

            foreach (var item in _sortedBdays)
                newData.Add(item.Key, item.Value);

            _sortedBdays = newData;
        }

        public void UpdateBirthday(ulong user, DateTime birthday)
        {
            if (!initialized) Initalize();

            if (BirthdayDictionary.ContainsKey(user))
                RemoveFromSortedList(user, BirthdayDictionary[user]);

            BirthdayDictionary[user] = birthday;

            if (!_sortedBdays.TryAdd(birthday, new List<ulong>() { user }))
                _sortedBdays[birthday].Add(user);
        }

        public bool RemoveBirthday(ulong user)
        {
            if (!initialized) Initalize();

            if (BirthdayDictionary.TryRemove(user, out DateTime date))
            {
                return RemoveFromSortedList(user, date);
            }

            return false;
        }

        private bool RemoveFromSortedList(ulong user, DateTime date)
        {
            if (!initialized) Initalize();

            if (_sortedBdays[date].Remove(user))
            {
                if (_sortedBdays[date].Count <= 0)
                    _sortedBdays.Remove(date);

                return true;
            }

            return false;
        }

        public DateTime? GetBirthday(ulong user)
        {
            if (!initialized) Initalize();

            if (BirthdayDictionary.TryGetValue(user, out DateTime value))
                return value;

            return null;
        }

        public List<ulong> GetBirthdaysOnToday()
        {
            if (!initialized) Initalize();

            var res = new List<ulong>(_sortedBdays.FirstOrDefault(
                    x => x.Key.Date.Month == Comparer.CurrentDay.Date.Month &&
                    x.Key.Date.Day == Comparer.CurrentDay.Date.Day
                ).Value ?? new List<ulong>());

            return res ?? new List<ulong>();
        }

        public SortedList<DateTime, List<ulong>> GetNextBirthdaysToLockout()
        {
            if (!initialized) Initalize();

            var res = new SortedList<DateTime, List<ulong>>(Comparer);

            var c = 0;
            foreach (var val in _sortedBdays)
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
            if (!initialized) Initalize();

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
            if (!initialized) Initalize();

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
