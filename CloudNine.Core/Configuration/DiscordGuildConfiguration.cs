using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Threading.Tasks;

using CloudNine.Core.Birthdays;
using CloudNine.Core.Quotes;

namespace CloudNine.Core.Configuration
{
    public class DiscordGuildConfiguration
    {
        [Key]
        public ulong Id { get; set; }

        public string? Prefix { get; set; }

        public BirthdayServerConfiguration BirthdayConfiguration { get; set; }
        public ConcurrentDictionary<int, Quote> Quotes { get; set; }
        public ConcurrentDictionary<string, Quote> HiddenQuotes { get; set; }
        public ConcurrentDictionary<ulong, SortedSet<int>> FavoriteQuotes { get; set; }

        [NotMapped]
        private HashSet<int>? _keys;
        [NotMapped]
        public HashSet<int> Keys
        {
            get
            {
                if (_keys is null)
                {
                    _keys = new HashSet<int>();
                    _keys.UnionWith(Quotes.Keys);
                }

                return _keys;
            }
        }

        public DiscordGuildConfiguration()
        {
            Id = 0;
            BirthdayConfiguration = new BirthdayServerConfiguration();
            Quotes = new ConcurrentDictionary<int, Quote>();
            HiddenQuotes = new ConcurrentDictionary<string, Quote>();
        }

        public Task<bool> AddQuote(Quote quote)
        {
            if(quote.Id == -1)
            { // Save hidden quote.
                if (quote.CustomId is null) return Task.FromResult(false);
                return Task.FromResult(HiddenQuotes.TryAdd(quote.CustomId, quote));
            }
            else
            { // Save standard quote.
                for (int i = 0; i <= Keys.Count; i++)
                {
                    if (!Keys.Contains(i))
                    {
                        quote.Id = i;
                        Keys.Add(i);
                        Quotes[i] = quote;
                        return Task.FromResult(true);
                    }
                }
            }

            return Task.FromResult(false);
        }

        public bool TryRemoveQuote(int id, out Quote? quote)
        {
            if (Keys.Remove(id))
                return Quotes.TryRemove(id, out quote);

            quote = null;
            return false;
        }

        public bool TryRemoveQuote(string id, out Quote? quote)
        {
            return HiddenQuotes.TryRemove(id, out quote);
        }
    }
}
