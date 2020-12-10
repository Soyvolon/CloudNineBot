using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Threading;

using DSharpPlus;

namespace CloudNine.Web.User
{
    public class LoginManager : ILoginManager
    {
        public string ClientSecret { get; private set; }

        private ConcurrentDictionary<string, Timer> Expirations { get; init; }
        private ConcurrentDictionary<string, string> ActiveLogins { get; init; }
        private ConcurrentDictionary<string, string> CodeTokens { get; init; }
        public ConcurrentDictionary<string, string> WaitingForVerification { get; init; }
        public DiscordRestClient Rest { get; init; }

        public LoginManager(DiscordRestClient rest, string clientSecret)
        {
            ActiveLogins = new();
            CodeTokens = new();
            Expirations = new();
            WaitingForVerification = new();
            Rest = rest;
            ClientSecret = clientSecret;
        }

        public Task<bool> AttemptRelogAsync(string state, out DiscordRestClient? userClient)
        {
            if(ActiveLogins.TryGetValue(state, out var code))
            {
                if(CodeTokens.TryGetValue(code, out var token))
                {
                    userClient = new(GetDiscordConfiguration(token));

                    return Task.FromResult(true);
                }
                else
                {
                    ActiveLogins.TryRemove(state, out _);
                }
            }

            userClient = null;
            return Task.FromResult(false);
        }

        public bool AttemptRelog(string state, out DiscordRestClient? userClient)
        {
            if (ActiveLogins.TryGetValue(state, out var code))
            {
                if (CodeTokens.TryGetValue(code, out var token))
                {
                    userClient = new(GetDiscordConfiguration(token));

                    return true;
                }
                else
                {
                    ActiveLogins.TryRemove(state, out _);
                }
            }

            userClient = null;
            return false;
        }

        private static DiscordConfiguration GetDiscordConfiguration(string token)
        {
            return new DiscordConfiguration()
            {
                Token = token,
                TokenType = TokenType.Bearer
            };
        }

        public void RegisterLogin(string state, string code, string token, TimeSpan expiration)
        {
            ActiveLogins[state] = code;
            CodeTokens[code] = token;

            SetupExparation(state, expiration);
        }

        public void RemoveRegistration(string state)
        {
            if(Expirations.TryRemove(state, out var t))
            {
                t.Change(TimeSpan.Zero, Timeout.InfiniteTimeSpan);
            }
            else
            {
                if (ActiveLogins.TryRemove(state, out var code))
                    CodeTokens.TryRemove(code, out _);
            }
        }

        public void SetupExparation(string state, TimeSpan expiresIn)
        {
            Expirations[state] = new Timer(async e =>
            {
                if (ActiveLogins.TryRemove(state, out var code))
                    CodeTokens.TryRemove(code, out _);

                if (Expirations.TryRemove(state, out var t))
                    await t.DisposeAsync();
            },
            null,
            expiresIn,
            Timeout.InfiniteTimeSpan);
        }
    }
}
