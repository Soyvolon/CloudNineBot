using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using CloudNine.Core.Blog;

using Microsoft.EntityFrameworkCore;

using Newtonsoft.Json;

namespace CloudNine.Core.Database
{
    public class BlogDatabaseModel : DbContext
    {
        public DbSet<BlogPost> BlogPosts { get; private set; }
        public DbSet<Showcase> Showcases { get; private set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            using FileStream fs = new FileStream(Path.Join("Config", "database_config.json"), FileMode.Open, FileAccess.Read, FileShare.Read);
            using StreamReader sr = new StreamReader(fs);
            var json = sr.ReadToEnd();

            var config = JsonConvert.DeserializeObject<DatabaseConfiguration>(json);

            optionsBuilder.UseSqlite(config.BlogDataSource)
                .EnableDetailedErrors();
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<BlogPost>()
                .Property(p => p.Tags)
                .HasConversion(
                    v => JsonConvert.SerializeObject(v),
                    v => JsonConvert.DeserializeObject<List<string>>(v) ?? new());

            modelBuilder.Entity<BlogPost>()
                .Property(p => p.Editors)
                .HasConversion(
                    v => JsonConvert.SerializeObject(v),
                    v => JsonConvert.DeserializeObject<List<string>>(v) ?? new());
        }
    }
}
