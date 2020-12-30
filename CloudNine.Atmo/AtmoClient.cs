using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using CloudNine.Atmo.Database;
using CloudNine.Atmo.Loaders;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace CloudNine.Atmo
{
    /// <summary>
    /// Entrypoint for all CloudNine.Atmo operations
    /// </summary>
    public class AtmoClient
    {
        public IServiceProvider Services { get; set; }

        public AtmoClient()
        {
            ServiceCollection collection = new ServiceCollection();

            collection.AddLogging(o => o.AddConsole())
                .AddSingleton<EncounterLoader>()
                .AddSingleton<EntityLoader>()
                .AddSingleton<ItemLoader>()
                .AddDbContext<AtmoDatabaseContext>();

            Services = collection.BuildServiceProvider();
        }
    }
}
