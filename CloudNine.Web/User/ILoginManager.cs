using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using DSharpPlus;

namespace CloudNine.Web.User
{
    public interface ILoginManager<T>
    {
        public Task<bool> AttemptRelogAsync(string state, out DiscordRestClient? userClient);
        public bool AttemptRelog(string state, out DiscordRestClient? userClient);
        public void RegisterLogin(string state, string code, string token, TimeSpan expiration);
        public void RemoveRegistration(string state);
        public void SetupExparation(string state, TimeSpan expiresIn);
    }
}
