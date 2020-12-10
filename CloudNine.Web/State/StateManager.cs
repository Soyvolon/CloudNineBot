using System;
using System.Security.Cryptography;
using System.Text;

namespace CloudNine.Web.State
{
    public class StateManager
    {
        private SHA256 _sha { get; init; }

        public StateManager()
        {
            _sha = SHA256.Create();
        }

        public string GetUniqueState()
        {
            var g = Guid.NewGuid().ToString();
            g = g.Replace("-", string.Empty);

            var s = _sha.ComputeHash(Encoding.ASCII.GetBytes(g));
            var res = Encoding.ASCII.GetString(s);

            return res;
        }
    }
}
