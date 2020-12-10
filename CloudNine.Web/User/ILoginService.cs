using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CloudNine.Web.User
{
    public interface ILoginService<T>
    {
        public string GetAuthUrl(string state);
        public void Logout();
        public Task<bool> Login(string code);

        public Task RestoreAsync();
        public void Restore();
    }
}
