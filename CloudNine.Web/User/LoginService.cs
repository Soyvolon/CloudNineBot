using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

using DSharpPlus;
using DSharpPlus.Entities;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

using Newtonsoft.Json.Linq;

using RestSharp;

namespace CloudNine.Web.User
{
    public class LoginService : ILoginService
    {
        public bool LoggedIn { get; set; }
        public DiscordGuild? ActiveGuild { get; private set; }
        public DiscordUser? ActiveUser { get; private set; }
        public Dictionary<ulong, DiscordGuild>? Guilds { get; private set; }
        public DiscordRestClient? UserRest { get; private set; }
        public string? State { get; private set; }
        public string? StateKey { get; private set; }

        private readonly ILogger _logger;
        private readonly IConfiguration _config;
        private readonly LoginManager _manager;
        private readonly HttpClient _http;

        public LoginService(IConfiguration config, ILogger<LoginService> logger, LoginManager manager, HttpClient http)
        {
            _config = config;
            _logger = logger;
            _manager = manager;
            _http = http;

            LoggedIn = false;
            ActiveGuild = null;
            ActiveUser = null;
            Guilds = null;
        }

        public string GetAuthUrl()
        {
            return $"https://discord.com/api/oauth2/authorize" +
                $"?client_id={_config["Client"]}" +
                $"&redirect_uri={_config["Login:Redirect"]}" +
                $"&response_type={_config["Login:Response"]}" +
                $"&scope={_config["Login:Scope"]}" +
                $"&state={StateKey}{State}" +
                $"&prompt={_config["Login:Prompt"]}";
        }

        public void Logout(string? state = null)
        {
            Guilds = null;
            ActiveGuild = null;
            ActiveUser = null;
            LoggedIn = false;
            State = null;

            UserRest?.Dispose();
            UserRest = null;

            if(state is not null)
            {
                _manager.RemoveRegistration(state);
            }
        }

        public async Task<bool> Login(string code)
        {
            using var tokenRequest = new HttpRequestMessage(HttpMethod.Post, "https://discord.com/api/oauth2/token");

            var tokenRequestContent = new List<KeyValuePair<string?, string?>>()
            {
                new("client_id", _config["Client"]),
                new("client_secret", _manager.ClientSecret),
                new("grant_type", "authorization_code"),
                new("code", code),
                new("redirect_uri", _config["Login:RawRedirect"]),
                new("scope", _config["Login:RawScope"])
            };

            tokenRequest.Content = new FormUrlEncodedContent(tokenRequestContent);

            var tokenResponse = await _http.SendAsync(tokenRequest);

            string? token = null;
            int expiration = 0;
            try
            {
                var res = await InitalizeUserRestClient(tokenResponse);

                if (res is null) return false;

                token = res.Item1;
                expiration = res.Item2;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to initalize User Rest client.");
            }

            if (token is not null && await LoadUserData())
            {
                if (Guilds is not null && ActiveUser is not null)
                {
                    _manager.RegisterLogin(State ?? "", code, token, TimeSpan.FromSeconds(expiration));
                    LoggedIn = true;
                    return true;
                }
            }

            return false;
        }

        private async Task<Tuple<string?, int>?> InitalizeUserRestClient(HttpResponseMessage tokenResponse)
        {
            if (tokenResponse.IsSuccessStatusCode)
            {
                var tokenJsonString = await tokenResponse.Content.ReadAsStringAsync();

                var tokenJson = JObject.Parse(tokenJsonString);

                if (tokenJson is not null)
                {
                    string? token = tokenJson["access_token"]?.ToString();
                    if (token is not null)
                    {
                        var dcfg = new DiscordConfiguration()
                        {
                            Token = token,
                            TokenType = TokenType.Bearer
                        };

                        UserRest = new DiscordRestClient(dcfg);
                        return new(token, tokenJson["expires_in"]?.ToObject<int>() ?? 0);
                    }
                }
            }

            return null;
        }

        public async Task<bool> LoadUserData()
        {
            if (UserRest is not null)
            {
                var guildTask = UserRest.GetCurrentUserGuildsAsync().ContinueWith(async (x) =>
                {
                    IReadOnlyList<DiscordGuild>? res = await x;

                    if (res is not null)
                    {
                        var resSet = res.ToHashSet();
                        var botRes = await _manager.Rest.GetCurrentUserGuildsAsync();
                        if(botRes is not null)
                        {
                            var botSet = botRes.ToHashSet();
                            resSet.IntersectWith(botSet);
                            Guilds = resSet.ToDictionary(x => x.Id);
                        }
                    }
                });

                var userTask = UserRest.GetCurrentUserAsync().ContinueWith(async (x) =>
                {
                    var res = await x;
                    ActiveUser = res;
                });

                var loadG = await guildTask; var loadU = await userTask;
                await loadG; await loadU;

                return true;
            }

            return false;
        }

        public async Task<bool> RestoreAsync(string state)
        {
            if(await _manager.AttemptRelogAsync(state, out var rest))
            {
                UserRest = rest;
                if (!await LoadUserData())
                {
                    Logout(state);
                    return false;
                }
                else
                {
                    if (ActiveUser is null || Guilds is null)
                    {
                        Logout(state);
                        return false;
                    }
                }

                LoggedIn = true;
                return true;
            }

            Logout(state);
            return false;
        }
        public void RegisterState(string key, string state)
        {
            _manager.WaitingForVerification[key] = state;
            State = state;
            StateKey = key;
        }

        public bool VerifyState(string returnedState)
        {
            var key = returnedState[..10];
            var state = returnedState[10..];

            if(_manager.WaitingForVerification.TryRemove(key, out string? localState))
            {
                if(state == localState)
                {
                    State = state;
                    StateKey = key;

                    return true;
                }
            }

            return false;
        }
    }
}
