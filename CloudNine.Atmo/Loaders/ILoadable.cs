using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CloudNine.Atmo.Loaders
{
    public interface ILoadable<T>
    {
        public bool LoadDefaultVars(T item);
    }
}
