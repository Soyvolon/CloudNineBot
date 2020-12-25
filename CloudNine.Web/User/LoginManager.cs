using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Threading;

using DSharpPlus;
using DSharpPlus.Entities;

namespace CloudNine.Web.User
{
    public class LoginManager : ILoginManager
    {
        public string ClientSecret { get; private set; }

        private ConcurrentDictionary<string, Timer> Expirations { get; init; }
        private ConcurrentDictionary<string, string> ActiveLogins { get; init; }
        private ConcurrentDictionary<string, string> CodeTokens { get; init; }
        public ConcurrentDictionary<string, string> WaitingForVerification { get; init; }
        public DiscordShardedClient Client { get; init; }

        public HashSet<ulong> InfinityIds { get; init; }

        public LoginManager(DiscordShardedClient client, string clientSecret, HashSet<ulong> infinityIds)
        {
            ActiveLogins = new();
            CodeTokens = new();
            Expirations = new();
            WaitingForVerification = new();
            this.Client = client;
            ClientSecret = clientSecret;
            InfinityIds = infinityIds;
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

        public bool GetGuildFromId(ulong id, out DiscordGuild? guild)
        {
            foreach (var shard in Client.ShardClients)
            {
                if (shard.Value.Guilds.TryGetValue(id, out guild))
                {
                    return true;
                }
            }

            guild = null;
            return false;
        }
    }
}
