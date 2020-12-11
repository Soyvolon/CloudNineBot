using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

using CloudNine.Web.State;
using CloudNine.Web.User;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;

namespace CloudNine.Web.Pages
{
    public class LoginModel : PageModel
    {
        private readonly StateManager _state;
        private readonly LoginService _login;
        private readonly Random _rand;
        private readonly ILogger _logger;

        public LoginModel(StateManager state, LoginService login, ILogger<LoginModel> logger)
        {
            _state = state;
            _login = login;
            _rand = new Random();
            _logger = logger;
        }

        public bool IsConnected { get; set; } = true;

        public async Task<IActionResult> OnGetAsync()
        {
            if(Request.Query.TryGetValue("start", out _))
            {
                _logger.LogInformation("Starting Login");
                return await StartLogin();
            }

            if (Request.Query.TryGetValue("logout", out _))
            {
                _logger.LogInformation("Start Logout");
                return await StartLogout();
            }

            if (Request.Query.TryGetValue("code", out var code) 
                && Request.Query.TryGetValue("state", out var state))
            {
                _logger.LogInformation("Finish Login");
                if(code.Count > 0 && state.Count > 0)
                    return await VerifyLogin(code[0], state[0]);
            }

            return Redirect("/");
        }

        private static CookieOptions GetCookieOptions()
        {
            return new()
            {
                MaxAge = TimeSpan.FromDays(1),
                SameSite = SameSiteMode.Lax,
                Secure = true,
                HttpOnly = false,
                IsEssential = true
            };
        }

        private async Task<IActionResult> StartLogin()
        {
            var state = _state.GetUniqueState().ToString();

            Response.Cookies.Append("login_state_key", state, GetCookieOptions());

            _login.RegisterState(await GetStatefulKey(10), state);

            var uri = _login.GetAuthUrl();

            _logger.LogInformation($"Redirecting to: {uri}");

            return Redirect(uri);
        }

        private async Task<IActionResult> StartLogout()
        {
            if (Request.Cookies.TryGetValue("login_state_key", out var state))
            {
                _login.Logout(state);

                Response.Cookies.Delete("login_state_key");
            }

            _logger.LogInformation("Logout Complete");

            return Redirect("/");
        }

        private async Task<IActionResult> VerifyLogin(string code, string state)
        {
            if (_login.VerifyState(state))
            {
                _logger.LogInformation("Completed Verify State");
                if (await _login.Login(code))
                {
                    _logger.LogInformation("Completed Login Process");
                    return Redirect("/dash");
                }
            }

            return Redirect("/");
        }

        private async Task<string> GetStatefulKey(int lenght)
        {
            string output = "";
            for (int i = 0; i < 10; )
            {
                var num = _rand.Next(48, 123);
                if (num >= 58 && num <= 64)
                    continue;
                if (num >= 91 && num <= 96)
                    continue;

                i++;
                output += (char)num;
            }

            return output;
        }
    }
}
