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
        public ulong SavedBy { get; set; }
        
        public SortedList<DateTime, string> Edits { get; set; }

        public SortedList<DateTime, string> Reverts { get; set; }

        public DateTime CreatedOn { get; private set; }

        public DateTime LastEdit { get; set; }

        public Warn() 
            : this("", 0, "", 0, null, null, DateTime.UtcNow, DateTime.UtcNow) { }

        public Warn(string key, ulong user_id) 
            : this(key, user_id, "", 0, null, null, DateTime.UtcNow, DateTime.UtcNow) { }

        public Warn(string key, ulong user_id, string message) 
            : this(key, user_id, message, 0, null, null, DateTime.UtcNow, DateTime.UtcNow) { }

        public Warn(string key, ulong user_id, string message, ulong saved_by) 
            : this(key, user_id, message, saved_by, null, null, DateTime.UtcNow, DateTime.UtcNow) { }

        public Warn(string key, ulong user_id, string message, ulong saved_by,
            SortedList<DateTime, string>? edits, SortedList<DateTime, string>? reverts,
            DateTime? created_on, DateTime? last_edit)
        {
            Key = key;
            UserId = user_id;
            Message = message;
            Edits = edits ?? new(new CustomDateTimeComparer());
            Reverts = reverts ?? new(new CustomDateTimeComparer());
            CreatedOn = created_on ?? DateTime.UtcNow;
            LastEdit = last_edit ?? DateTime.UtcNow;
        }

        public void AddEdit(string newContent)
        {
            Edits.Add(LastEdit, Message);
            Message = newContent;
            LastEdit = DateTime.UtcNow;
        }

        public bool Undo()
        {
            KeyValuePair<DateTime, string> edit = Edits.FirstOrDefault();
            if (edit.Value is null)
                return false;

            Edits.Remove(edit.Key);
            Reverts.Add(LastEdit, Message);
            Message = edit.Value;
            LastEdit = DateTime.UtcNow;

            return true;
        }

        public bool Redo()
        {
            KeyValuePair<DateTime, string> revert = Reverts.FirstOrDefault();
            if (revert.Value is null)
                return false;

            Reverts.Remove(revert.Key);
            Edits.Add(LastEdit, Message);
            Message = revert.Value;
            LastEdit = DateTime.UtcNow;

            return true;
        }

        public override bool Equals(object? obj)
            => this.Equals(obj as Warn);

        public override int GetHashCode()
            => Key.GetHashCode();

        public bool Equals(Warn? other)
            => other?.GetHashCode().Equals(this.GetHashCode()) ?? false;
    }
}
