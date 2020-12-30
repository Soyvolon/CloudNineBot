using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CloudNine.Atmo.Loaders
{
    internal class EntityLoader : BaseLoader
    {
        internal string EntityPath { get; init; }

        public EntityLoader() : base()
        {
            EntityPath = Path.Join(BasePath, "Encounters");

            if (!Directory.Exists(EntityPath))
                Directory.CreateDirectory(EntityPath);
        }
    }
}
