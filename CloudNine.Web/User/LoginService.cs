using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using DSharpPlus.Entities;

using Microsoft.Extensions.Logging;

namespace CloudNine.Web.User
{
    public class LoginService
    {
        public bool LoggedIn { get; set; }
        public DiscordGuild? ActiveGuild { get; private set; }

        private string? _state;
        private readonly ILogger _logger;

        public LoginService(ILogger<LoginService> logger)
        {
            _logger = logger;
            _state = null;

            LoggedIn = false;
            ActiveGuild = null;
        }

        public void StartLoginAttempt(string state)
            => _state = state;

        public bool VerifyState(string state)
            => _state?.Equals(state) ?? false;
    }
}
