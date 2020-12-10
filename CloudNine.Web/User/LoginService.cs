using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

using DSharpPlus;
using DSharpPlus.Entities;

using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

using Newtonsoft.Json.Linq;

using RestSharp;

namespace CloudNine.Web.User
{
    public class LoginService : ILoginService
    {
        #region Loading bar stuff
        public int LoadState = 0;
        public event Action LoadIncreased;
        public const int MaxLoad = 12;
        private Dispatcher dispatch;
        #endregion

        public bool LoggedIn { get; set; }
        public DiscordGuild? ActiveGuild { get; private set; }
        public DiscordUser? ActiveUser { get; private set; }
        public IReadOnlyDictionary<ulong, DiscordGuild>? Guilds { get; private set; }
        public SortedList<string, DiscordGuild>? OrderedGuilds { get; private set; }
        public DiscordRestClient? UserRest { get; private set; }
        public string? State { get; private set; }
        public string? StateKey { get; private set; }

        public string PermString { get; private set; }

        public DiscordMember? ActiveMember { get; private set; }

        private readonly ILogger _logger;
        private readonly IConfiguration _config;
        private readonly LoginManager _manager;
        private readonly HttpClient _http;

        public LoginService(IConfiguration config, ILogger<LoginService> logger, LoginManager manager, HttpClient http)
        {
            dispatch = Dispatcher.CreateDefault();

            _config = config;
            _logger = logger;
            _manager = manager;
            _http = http;

            LoggedIn = false;
            ActiveGuild = null;
            ActiveUser = null;
            Guilds = null;
        }

        public void StepLoad()
        {
            LoadState++;
            if (LoadIncreased is not null)
                dispatch.InvokeAsync(LoadIncreased);
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
            LoadState = 0;

            UserRest?.Dispose();
            UserRest = null;

            if (state is not null)
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

            StepLoad();

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

            StepLoad();

            if (token is not null && await LoadUserData())
            {
                if (Guilds is not null && ActiveUser is not null)
                {
                    _manager.RegisterLogin(State ?? "", code, token, TimeSpan.FromSeconds(expiration));
                    StepLoad();
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

                    StepLoad();

                    if (res is not null)
                    {
                        var resSet = res.ToHashSet();
                        var botRes = await _manager.Rest.GetGuildAsync();

                        StepLoad();

                        if (botRes is not null)
                        {
                            var botSet = botRes.ToHashSet();

                            var finalSet = new HashSet<DiscordGuild>();

                            foreach (var v in resSet)
                            {
                                if (botRes.Any(x => x.Id == v.Id))
                                {
                                    finalSet.Add(v);
                                }
                            }

                            StepLoad();

                            Guilds = finalSet.ToDictionary(x => x.Id);
                            OrderedGuilds = new SortedList<string, DiscordGuild>(Guilds.ToDictionary(x => x.Value.Name, y => y.Value));
                        }
                    }
                });

                var userTask = UserRest.GetCurrentUserAsync().ContinueWith(async (x) =>
                {
                    var res = await x;

                    StepLoad();

                    ActiveUser = res;
                });

                var loadG = await guildTask;
                StepLoad();
                var loadU = await userTask;
                StepLoad();
                await loadG;
                StepLoad();
                await loadU;
                StepLoad();

                return true;
            }

            return false;
        }

        public async Task<bool> RestoreAsync(string state)
        {
            StepLoad();

            if (await _manager.AttemptRelogAsync(state, out var rest))
            {
                StepLoad();
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

                StepLoad();
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

            if (_manager.WaitingForVerification.TryRemove(key, out string? localState))
            {
                if (state == localState)
                {
                    State = state;
                    StateKey = key;

                    return true;
                }
            }

            return false;
        }

        private string MaxPermissionString(byte perms)
        {
            if ((perms & 0x8) == 0x8)
                return "Administrator";

            if ((perms & 0x2) == 0x2
                || (perms & 0x4) == 0x4
                || (perms & 0x10) == 0x10
                || (perms & 0x20) == 0x20
                || (perms & 0x2000) == 0x2000
                || (perms & 0x10000000) == 0x10000000)
                return "Moderator";

            return "Member";
        }

        public void SetPermString()
        {
            if (ActiveGuild is not null)
            {
                if (ActiveMember is not null)
                {
                    if (ActiveGuild.IsOwner)
                    {
                        PermString = "Owner";
                        return;
                    }

                    if (ActiveMember.Roles is not null)
                    {
                        Permissions perms = 0x0;
                        foreach (var role in ActiveMember.Roles)
                            perms |= role.Permissions;

                        PermString = MaxPermissionString((byte)perms);
                        return;
                    }
                }
            }

            PermString = "n/a";
        }

        public async Task<bool> SetActiveGuild(ulong id)
        {
            if(Guilds is not null && Guilds.TryGetValue(id, out var g))
            {
                ActiveGuild = g;
                ActiveMember = await _manager.Rest.GetGuildMemberAsync(ActiveGuild.Id, ActiveUser?.Id ?? default);
                SetPermString();
                return true;
            }
            else
            {
                ActiveGuild = null;
                ActiveMember = null;
                return false;
            }
        }
    }
}
