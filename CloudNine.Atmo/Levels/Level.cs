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
    public abstract class Level : StateMachine
    {
        public Level(CommandContext ctx, int MaxUsers) : base(ctx)
        {
            CreatedBy = ctx.Member.Id;
            ActiveUsers = new();
        }

        public int MaxPlayers { get; internal set; }
        public ulong CreatedBy { get; internal set; }
        public List<ulong> ActiveUsers { get; internal set; }
    }
}
