using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CloudNine.Web.User
{
    public interface ILoginService
    {
        public string GetAuthUrl();
        public void Logout(string? state);
        public Task<bool> Login(string code);

        public Task<bool> RestoreAsync(string state);
        public void RegisterState(string key, string state);
        public bool VerifyState(string returnedState);
    }
}
