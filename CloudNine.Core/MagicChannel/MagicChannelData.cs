using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CloudNine.Core.MagicChannel
{
    public class MagicChannelData
    {
        public ulong RoleToAssign { get; set; }
        public HashSet<ulong> RolesToIgnore { get; set; }
        public HashSet<ulong> UsersToIgnore { get; set; }
        [Key]
        public ulong ChannelId { get; set; }
        public TimeSpan Interval { get; set; }
        public decimal MemberPercent { get; set; }
        public int RemoveAfterMessages { get; set; }
        public DateTime LastRun { get; set; } = DateTime.MinValue;

        public MagicChannelData()
        {
            RolesToIgnore = new();
            UsersToIgnore = new();
            RemoveAfterMessages = 0;
        }
    }
}
