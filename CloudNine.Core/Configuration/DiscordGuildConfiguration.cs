using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;
using System.Threading.Tasks;

using CloudNine.Core.Birthdays;
using CloudNine.Core.Quotes;

using Microsoft.EntityFrameworkCore.Storage;

namespace CloudNine.Core.Configuration
{
    public class DiscordGuildConfiguration
    {
        [Key]
        public ulong Id { get; set; }

        public string Prefix { get; set; }

        public BirthdayServerConfiguration BirthdayConfiguration { get; set; }
        public ConcurrentDictionary<int, Quote> Quotes { get; set; }

        [NotMapped]
        private HashSet<int>? _keys;
        [NotMapped]
        public HashSet<int> Keys
        {
            get
            {
                if(_keys is null)
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
            Prefix = ".";
            BirthdayConfiguration = new BirthdayServerConfiguration();
            Quotes = new ConcurrentDictionary<int, Quote>();
        }

        public Task<bool> AddQuote(Quote quote)
        {
            for(int i = 0; i <= Keys.Count; i++)
            {
                if(!Keys.Contains(i))
                {
                    quote.Id = i;
                    Keys.Add(i);
                    Quotes[i] = quote;
                    return Task.FromResult(true);
                }
            }

            return Task.FromResult(false);
        }

        public bool TryRemoveQuote(int id, out Quote? quote)
        {
            if(Keys.Remove(id))
                return Quotes.TryRemove(id, out quote);

            quote = null;
            return false;
        }
    }
}
