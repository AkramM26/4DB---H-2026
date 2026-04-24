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
        public DbSet<Territory> Territories => Set<Territory>();
        public DbSet<MilitaryDetachment> MilitaryDetachments => Set<MilitaryDetachment>();
        public DbSet<Installation> Installations => Set<Installation>();
        public DbSet<PlayerAction> PlayerActions => Set<PlayerAction>();
        public DbSet<TurnSnapShot> TurnSnapShots => Set<TurnSnapShot>();
        public DbSet<CombatEvent> CombatEvents => Set<CombatEvent>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Game>()
                .HasQueryFilter(g => g.Id == CurrentGameId);

            modelBuilder.Entity<Player>()
                .HasQueryFilter(p => p.GameId == CurrentGameId);

            modelBuilder.Entity<MilitaryDetachment>()
                .HasQueryFilter(m => m.GameId == CurrentGameId);

            modelBuilder.Entity<Installation>()
                .HasQueryFilter(i => i.GameId == CurrentGameId);

            //modelBuilder.Entity<Installation>()
            //    .HasOne(i => i.Game)
            //    .WithMany(g => g.Installations)
            //    .HasForeignKey(i => i.GameId)
            //    .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Territory>()
                .HasQueryFilter(t => t.GameId == CurrentGameId);

            modelBuilder.Entity<PlayerAction>()
                .HasQueryFilter(a => a.GameId == CurrentGameId);

            modelBuilder.Entity<TurnSnapShot>()
                .HasQueryFilter(t => t.GameId == CurrentGameId);

            modelBuilder.Entity<CombatEvent>()
                .HasQueryFilter(c => c.GameId == CurrentGameId);

            modelBuilder.Entity<CombatEvent>()
                .HasOne(c => c.Game)
                .WithMany(g => g.CombatEvents)
                .HasForeignKey(c => c.GameId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<PlayerAction>()
                .HasOne(p => p.Game)
                .WithMany(g => g.PlayerActions)
                .HasForeignKey(p => p.GameId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Territory>()
                .HasIndex(t => new { t.GameId, t.PositionX, t.PositionY })
                .IsUnique();

            //modelBuilder.Entity<MilitaryDetachment>()
            //    .HasOne(m => m.Game)
            //    .WithMany(g => g.MilitaryDetachments)
            //    .HasForeignKey(m => m.GameId)
            //    .OnDelete(DeleteBehavior.Restrict);

            //modelBuilder.Entity<MilitaryDetachment>()
            //    .HasOne(m => m.Territory)
            //    .WithOne(t => t.MilitaryDetachment)
            //    .HasForeignKey<MilitaryDetachment>(m => m.TerritoryId)
            //    .OnDelete(DeleteBehavior.Restrict);

            //modelBuilder.Entity<Installation>()
            //    .HasOne(i => i.Game)
            //    .WithMany(g => g.Installations)
            //    .HasForeignKey(i => i.GameId)
            //    .OnDelete(DeleteBehavior.Restrict);

            //modelBuilder.Entity<Installation>()
            //    .HasOne(i => i.Territory)
            //    .WithOne(t => t.Installation)
            //    .HasForeignKey<Installation>(i => i.TerritoryId)
            //    .OnDelete(DeleteBehavior.Restrict);

            base.OnModelCreating(modelBuilder);
        }
    }
}
