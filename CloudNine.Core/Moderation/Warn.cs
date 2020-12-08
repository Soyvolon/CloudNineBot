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
    public class Warn : IEquatable<Warn>
    {
        public string Key { get; set; }

        public ulong UserId { get; set; }

        public string Message { get; set; }
        
        public SortedList<DateTime, string> Edits { get; set; }

        public SortedList<DateTime, string> Reverts { get; set; }

        public DateTime CreatedOn { get; private set; }

        public DateTime LastEdit { get; set; }

        public Warn() : this("", 0, "", new(), new(), DateTime.UtcNow, DateTime.UtcNow) { }

        public Warn(string key, ulong user_id) : this(key, user_id, "", new(), new(), DateTime.UtcNow, DateTime.UtcNow) { }

        public Warn(string key, ulong user_id, string message) : this(key, user_id, message, new(), new(), DateTime.UtcNow, DateTime.UtcNow) { }

        public Warn(string key, ulong user_id, string message,
            SortedList<DateTime, string> edits, SortedList<DateTime, string> reverts,
            DateTime? created_on, DateTime? last_edit)
        {
            Key = key;
            UserId = user_id;
            Message = message;
            Edits = edits;
            Reverts = reverts;
            CreatedOn = created_on ?? DateTime.UtcNow;
            LastEdit = last_edit ?? DateTime.UtcNow;
        }

        public override bool Equals(object? obj)
            => this.Equals(obj as Warn);

        public override int GetHashCode()
            => Key.GetHashCode();

        public bool Equals(Warn? other)
            => other?.GetHashCode().Equals(this.GetHashCode()) ?? false;
    }
}
