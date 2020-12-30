using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CloudNine.Atmo.Loaders
{
    internal abstract class BaseLoader
    {
        internal const string BasePath = "Data";

        public BaseLoader()
        {
            if (!Directory.Exists(BasePath))
                Directory.CreateDirectory(BasePath);
        }

        internal class DuplicateItemIdException : Exception
        {
            public DuplicateItemIdException() : base() { }
            public DuplicateItemIdException(string? message) : base(message) { }
        }
    }
}
