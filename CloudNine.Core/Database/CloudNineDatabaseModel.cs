using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;

using CloudNine.Core.Birthdays;
using CloudNine.Core.Configuration;
using CloudNine.Core.Moderation;
using CloudNine.Core.Quotes;

using Microsoft.EntityFrameworkCore;

using Newtonsoft.Json;

namespace CloudNine.Core.Database
{
    public class CloudNineDatabaseModel : DbContext
    {
        public DbSet<DiscordGuildConfiguration> ServerConfigurations { get; set; }
        public DbSet<ModCore> Moderation { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            using FileStream fs = new FileStream(Path.Join("Config", "database_config.json"), FileMode.Open);
            using StreamReader sr = new StreamReader(fs);
            var json = sr.ReadToEnd();

            var config = JsonConvert.DeserializeObject<DatabaseConfiguration>(json);

            optionsBuilder.UseSqlite(config.DataSource)
                .EnableDetailedErrors();
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<DiscordGuildConfiguration>()
                .Property(b => b.BirthdayConfiguration)
                .HasConversion(
                    v => JsonConvert.SerializeObject(v),
                    v => JsonConvert.DeserializeObject<BirthdayServerConfiguration>(v) ?? new());

            modelBuilder.Entity<DiscordGuildConfiguration>()
                .Property(b => b.Quotes)
                .HasConversion(
                    v => JsonConvert.SerializeObject(v),
                    v => JsonConvert.DeserializeObject<ConcurrentDictionary<int, Quote>>(v) ?? new());

            modelBuilder.Entity<DiscordGuildConfiguration>()
                .Property(b => b.HiddenQuotes)
                .HasConversion(
                    v => JsonConvert.SerializeObject(v),
                    v => JsonConvert.DeserializeObject<ConcurrentDictionary<string, Quote>>(v) ?? new());

            modelBuilder.Entity<ModCore>()
                .Property(b => b.WarnSet)
                .HasConversion(
                    v => JsonConvert.SerializeObject(v),
                    v => JsonConvert.DeserializeObject<HashSet<Warn>>(v) ?? new());
        }
    }
}
