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
        public SortedList<DateTime, List<ulong>> SortedBirthdays { get; private set; }
        [JsonIgnore]
        private BirthdayDateComparer Comparer;

        public BirthdayServerConfiguration()
        {
            Comparer = new BirthdayDateComparer(DateTime.UtcNow);
            BirthdayDictionary = new ConcurrentDictionary<ulong, DateTime>();
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
            if (BirthdayDictionary.ContainsKey(user))
                RemoveFromSortedList(user, BirthdayDictionary[user]);

            BirthdayDictionary[user] = birthday;

            if (!SortedBirthdays.TryAdd(birthday, new List<ulong>() { user }))
                SortedBirthdays[birthday].Add(user);
        }

        public bool RemoveBirthday(ulong user)
        {
            if (BirthdayDictionary.TryRemove(user, out DateTime date))
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
            foreach (var val in SortedBirthdays)
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
