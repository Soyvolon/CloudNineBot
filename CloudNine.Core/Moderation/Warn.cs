using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Newtonsoft.Json;

namespace CloudNine.Core.Moderation
{
    public class Warn
    {
        [JsonProperty("user_id")]
        public ulong UserId { get; set; }
        [JsonProperty("message")]
        public string Message { get; set; }
        [JsonProperty("edits")]
        public ConcurrentStack<string> Edits { get; set; }
        [JsonProperty("reverts")]
        public ConcurrentStack<string> Reverts { get; set; }

        public Warn() : this(0, "", new(), new()) { }

        public Warn(ulong user_id) : this(user_id, "", new(), new()) { }

        public Warn(ulong user_id, string message) : this(user_id, message, new(), new()) { }

        [JsonConstructor]
        public Warn(ulong user_id, string message, 
            ConcurrentStack<string> edits, ConcurrentStack<string> reverts)
        {
            UserId = user_id;
            Message = message;
            Edits = edits;
            Reverts = reverts;
        }
    }
}
