using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using DSharpPlus;

namespace CloudNine.Web.User
{
    public class LoginManager : ILoginManager<LoginManager>
    {
        public string ClientSecret { get; private set; }

        private ConcurrentDictionary<string, string> activeLogins { get; init; }
        private DiscordRestClient Rest { get; init; }

        public LoginManager(DiscordRestClient rest, string clientSecret)
        {
            activeLogins = new();
            Rest = rest;
            ClientSecret = clientSecret;
        }

        public Task<bool> AttemptRelogAsync(string state, out string? code)
        {
            if(activeLogins.TryGetValue(state, out code))
            {

            }

            return Task.FromResult(false);
        }

        public bool AttemptRelog(string state, out string? code)
        {
            code = null;
            return false;
        }
    }
}
