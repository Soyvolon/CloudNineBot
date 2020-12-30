using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CloudNine.Atmo.Loaders
{
    internal class EncounterLoader : BaseLoader
    {
        public string EncounterPath { get; init; }

        public EncounterLoader() : base()
        {
            EncounterPath = Path.Join(BasePath, "Encounters");

            if (!Directory.Exists(EncounterPath))
                Directory.CreateDirectory(EncounterPath);
        }
    }
}
