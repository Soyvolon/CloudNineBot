using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CloudNine.Web.State
{
    public interface IRefreshRequestService
    {
        public event Action RefreshRequested;
        public void CallRequestRefresh();
    }
}
