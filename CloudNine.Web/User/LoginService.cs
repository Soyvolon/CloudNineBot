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
    public class LoginService : ILoginService<LoginManager>
    {
        public bool LoggedIn { get; set; }
        public DiscordGuild? ActiveGuild { get; private set; }
        public DiscordUser? ActiveUser { get; private set; }
        public Dictionary<ulong, DiscordGuild> Guilds { get; private set; }
        public DiscordRestClient? UserRest { get; private set; }

        private string? _code;

        private readonly ILogger _logger;
        private readonly IConfiguration _config;
        private readonly LoginManager _manager;
        private readonly HttpClient _http;

        public LoginService(IConfiguration config, ILogger<LoginService> logger, LoginManager manager, HttpClient http)
        {
            _config = config;
            _logger = logger;
            _code = null;
            _manager = manager;
            _http = http;

            LoggedIn = false;
            ActiveGuild = null;
            ActiveUser = null;

            Guilds = new Dictionary<ulong, DiscordGuild>();
        }

        public string GetAuthUrl(string state)
        {
            return $"https://discord.com/api/oauth2/authorize" +
                $"?client_id={_config["Client"]}" +
                $"&redirect_uri={_config["Login:Redirect"]}" +
                $"&response_type={_config["Login:Response"]}" +
                $"&scope={_config["Login:Scope"]}" +
                $"&state={state}" +
                $"&prompt={_config["Login:Prompt"]}";
        }

        public void Logout()
        {
            _code = null;
            Guilds.Clear();
            ActiveGuild = null;
            LoggedIn = false;
        }

        public async Task<bool> Login(string code)
        {
            _code = code;
            LoggedIn = true;

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

            try
            {
                await InitalizeUserRestClient(tokenResponse);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to initalize User Rest client.");
            }

            if(UserRest is not null)
            {
                var guildTask = UserRest.GetCurrentUserGuildsAsync().ContinueWith(async (x) =>
                {
                    IReadOnlyList<DiscordGuild>? res = await x;

                    if(res is not null)
                        Guilds = res.ToDictionary(x => x.Id);
                });
                var userTask = UserRest.GetCurrentUserAsync().ContinueWith(async (x) =>
                {
                    var res = await x;
                    ActiveUser = res;
                });

                await guildTask; await userTask;

                return true;
            }

            return false;
        }

        private async Task InitalizeUserRestClient(HttpResponseMessage tokenResponse)
        {
            if (tokenResponse.IsSuccessStatusCode)
            {
                var tokenJsonString = await tokenResponse.Content.ReadAsStringAsync();

                var tokenJson = JObject.Parse(tokenJsonString);

                if (tokenJson is not null)
                {
                    var dcfg = new DiscordConfiguration()
                    {
                        Token = tokenJson["access_token"]?.ToString() ?? "",
                        TokenType = TokenType.Bearer
                    };

                    UserRest = new DiscordRestClient(dcfg);
                }
            }
        }

        public Task RestoreAsync() => throw new NotImplementedException();
        public void Restore() => throw new NotImplementedException();
    }
}
