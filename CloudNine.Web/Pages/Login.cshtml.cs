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

namespace CloudNine.Web.Pages
{
    public class LoginModel : PageModel
    {
        private readonly StateManager _state;
        private readonly LoginService _login;
        private readonly Random _rand;

        public LoginModel(StateManager state, LoginService login)
        {
            _state = state;
            _login = login;
            _rand = new Random();
        }

        public bool IsConnected { get; set; } = true;

        public async Task<IActionResult> OnGetAsync()
        {
            if(Request.Query.TryGetValue("start", out _))
            {
                return await StartLogin();
            }

            if (Request.Query.TryGetValue("logout", out _))
            {
                return await StartLogout();
            }

            if (Request.Query.TryGetValue("code", out var code) 
                && Request.Query.TryGetValue("state", out var state))
            {
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
                HttpOnly = false
            };
        }

        private async Task<IActionResult> StartLogin()
        {
            var state = _state.GetUniqueState().ToString();

            Response.Cookies.Append("login_state_key", state, GetCookieOptions());

            _login.RegisterState(await GetStatefulKey(10), state);

            var uri = _login.GetAuthUrl();

            return Redirect(uri);
        }

        private async Task<IActionResult> StartLogout()
        {
            if (Request.Cookies.TryGetValue("login_state_key", out var state))
            {
                _login.Logout(state);

                Response.Cookies.Delete("login_state_key");
            }

            return Redirect("/");
        }

        private async Task<IActionResult> VerifyLogin(string code, string state)
        {
            if (_login.VerifyState(state))
            {
                if (await _login.Login(code))
                {
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
