using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CloudNine.Web.State
{
    public class RefreshRequestService : IRefreshRequestService
    {
        public event Action RefreshRequested;

        public void CallRequestRefresh()
        {
            RefreshRequested?.Invoke();
        }
    }
}
