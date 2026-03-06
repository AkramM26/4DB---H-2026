using HugoLand.Core.Domain;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HugoLand.Core.Data
{
    public class HugoLandContext(DbContextOptions<HugoLandContext> options) : DbContext(options)
    {
        public Guid CurrentGameId { get; set; }

        public DbSet<Game> Games => Set<Game>();
        public DbSet<Player> Players => Set<Player>();
        public DbSet<Territory> Territories=> Set<Territory>();
        public DbSet<MilitaryDetachment> MilitaryDetachments => Set<MilitaryDetachment>();
        public DbSet<Installation> Installations => Set<Installation>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Player>()
                .HasQueryFilter(p => p.GameId == CurrentGameId);

            modelBuilder.Entity<MilitaryDetachment>()
                .HasQueryFilter(m => m.GameId == CurrentGameId);

            modelBuilder.Entity<Installation>()
                .HasQueryFilter(i => i.GameId == CurrentGameId);

            modelBuilder.Entity<Territory>()
                .HasQueryFilter(t => t.GameId == CurrentGameId);

            base.OnModelCreating(modelBuilder);
        }
    }
}
