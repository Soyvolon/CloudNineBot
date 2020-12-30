using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using CloudNine.Atmo.Entities.Player;

using Microsoft.EntityFrameworkCore;

using Newtonsoft.Json;

namespace CloudNine.Atmo.Database
{
    public class AtmoDatabaseContext : DbContext
    {
        public DbSet<Player> Players { get; set; }


        protected override void OnConfiguring(DbContextOptionsBuilder optionsBUilder)
        {
            using FileStream fs = new FileStream(Path.Join("Config", "database_config.json"), FileMode.Open);
            using StreamReader sr = new(fs);
            var json = sr.ReadToEnd();

            var config = JsonConvert.DeserializeObject<AtmoDatabaseConfiguration>(json);

            optionsBUilder.UseSqlite(config.DataSource)
                .EnableDetailedErrors();
        }
    }
}
