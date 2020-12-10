using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CloudNine.Web.User
{
    public interface ILoginManager<T>
    {
        public Task<bool> AttemptRelogAsync(string state, out string? code);
        public bool AttemptRelog(string state, out string? code);
    }
}
