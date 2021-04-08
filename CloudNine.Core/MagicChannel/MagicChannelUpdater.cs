using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CloudNine.Core.MagicChannel
{
    public class MagicChannelUpdater
    {
        public HashSet<ulong> IgnoreRolesToAdd { get; set; } = new();
        public HashSet<ulong> IgnoreRolesToRemove { get; set; } = new();
        public HashSet<ulong> IgnoreUsersToAdd { get; set; } = new();
        public HashSet<ulong> IgnoreUsersToRemove { get; set; } = new();
        public TimeSpan? Interval { get; set; } = null;
        public decimal? Percentage { get; set; } = null;
        public ulong? Role { get; set; } = null;
        public int? RemoveAfterMessages { get; set; } = null;
        public DateTime? LastRun { get; set; } = null;
    }
}
