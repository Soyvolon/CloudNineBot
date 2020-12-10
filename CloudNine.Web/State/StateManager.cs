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
            var res = g.Replace("-", string.Empty);

            return res;
        }
    }
}
