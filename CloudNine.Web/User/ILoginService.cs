using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CloudNine.Web.User
{
    public interface ILoginService<T>
    {
        public string GetAuthUrl(string state);
        public void Logout(string? state);
        public Task<bool> Login(string state, string code);

        public Task<bool> RestoreAsync(string state);
    }
}
