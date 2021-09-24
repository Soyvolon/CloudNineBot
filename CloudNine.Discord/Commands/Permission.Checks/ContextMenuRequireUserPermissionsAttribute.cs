using DSharpPlus;
using DSharpPlus.SlashCommands;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CloudNine.Discord.Commands.Permission.Checks
{
    public class ContextMenuRequireUserPermissionsAttribute : ContextMenuCheckBaseAttribute
    {
        public Permissions Perms { get; set; }

        public ContextMenuRequireUserPermissionsAttribute(Permissions permissions)
            => Perms = permissions;

        public override Task<bool> ExecuteChecksAsync(ContextMenuContext ctx)
        {
            return Task.FromResult(ctx.Member.Permissions.HasFlag(Perms));
        }
    }
}
