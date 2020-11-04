using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

using CloudNine.Core.Birthdays;
using CloudNine.Core.Configuration;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

using Newtonsoft.Json;

namespace CloudNine.Core.Database
{
    public class CloudNineDatabaseModel : DbContext
    {
        public DbSet<DiscordServerConfiguration> ServerConfigurations { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            using FileStream fs = new FileStream("Config/database_config.json", FileMode.Open);
            using StreamReader sr = new StreamReader(fs);
            var json = sr.ReadToEnd();

            var config = JsonConvert.DeserializeObject<DatabaseConfiguration>(json);

            optionsBuilder.UseSqlite(config.DataSource)
                .EnableDetailedErrors();
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<DiscordServerConfiguration>()
                .Property(b => b.BirthdayConfiguration)
                .HasConversion(
                    v => JsonConvert.SerializeObject(v),
                    v => JsonConvert.DeserializeObject<BirthdayServerConfiguration>(v) ?? new BirthdayServerConfiguration());
        }
    }
}
