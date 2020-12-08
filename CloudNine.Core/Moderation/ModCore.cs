using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CloudNine.Core.Moderation
{
    public class ModCore
    {
        [Key]
        public ulong GuildId { get; set; }

        public ConcurrentDictionary<ulong, ConcurrentStack<Warn>> Warns { get; set; }

        public ModCore() : this(0, new()) { }

        public ModCore(ulong guildId) : this(guildId, new()) { }

        public ModCore(ulong guildId, ConcurrentDictionary<ulong, ConcurrentStack<Warn>> warns)
        {
            GuildId = guildId;
            Warns = warns;
        }
    }
}
