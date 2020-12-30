using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using CloudNine.Atmo.State;

using DSharpPlus.CommandsNext;
using DSharpPlus.EventArgs;

namespace CloudNine.Atmo.Levels
{
    public abstract class BaseLevel : StateMachine
    {
        public BaseLevel(CommandContext ctx, int MaxUsers) : base(ctx)
        {
            CreatedBy = ctx.Member.Id;
            ActiveUsers = new();
        }

        public int MaxPlayers { get; internal set; }
        public ulong CreatedBy { get; internal set; }
        public HashSet<ulong> ActiveUsers { get; internal set; }
    }
}
