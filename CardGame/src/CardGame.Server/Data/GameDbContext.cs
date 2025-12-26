using Microsoft.EntityFrameworkCore;
using CardGame.Common.Models;

namespace CardGame.Server.Data
{
    public class GameDbContext : DbContext
    {
        public GameDbContext(DbContextOptions<GameDbContext> options)
            : base(options)
        {
        }

        public DbSet<Player> Players { get; set; }
       // public DbSet<Battle> Battles { get; set; }
       // public DbSet<Clan> Clans { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Players
            modelBuilder.Entity<Player>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.Username).IsUnique();
                entity.HasIndex(e => e.Email).IsUnique();
                entity.HasIndex(e => e.Rating);
              //  entity.HasIndex(e => e.ClanId);

                entity.Property(e => e.CreatedAt)
                    .HasDefaultValueSql("NOW()");

                entity.Property(e => e.Rating)
                    .HasDefaultValue(1000);

                entity.Property(e => e.Level)
                    .HasDefaultValue(1);

                // JSON для хранения формаций
                entity.Property(e => e.DefenseFormationJson)
                    .HasColumnType("jsonb")
                    .HasDefaultValue("{}");

                //// Связь с кланами
                //entity.HasOne(e => e.Clan)
                //    .WithMany(c => c.Members)
                //    .HasForeignKey(e => e.ClanId)
                //    .OnDelete(DeleteBehavior.SetNull);
            });

            // Battles
            //modelBuilder.Entity<Battle>(entity =>
            //{
            //    entity.HasKey(e => e.Id);
            //    entity.HasIndex(e => e.AttackerId);
            //    entity.HasIndex(e => e.DefenderId);
            //    entity.HasIndex(e => e.CreatedAt);

            //    entity.Property(e => e.BattleLog)
            //        .HasColumnType("jsonb");

            //    entity.Property(e => e.CreatedAt)
            //        .HasDefaultValueSql("NOW()");

            //    // Связи
            //    entity.HasOne(e => e.Attacker)
            //        .WithMany()
            //        .HasForeignKey(e => e.AttackerId)
            //        .OnDelete(DeleteBehavior.Restrict);

            //    entity.HasOne(e => e.Defender)
            //        .WithMany()
            //        .HasForeignKey(e => e.DefenderId)
            //        .OnDelete(DeleteBehavior.Restrict);
            //});

            //// Clans
            //modelBuilder.Entity<Clan>(entity =>
            //{
            //    entity.HasKey(e => e.Id);
            //    entity.HasIndex(e => e.Name).IsUnique();

            //    entity.Property(e => e.CreatedAt)
            //        .HasDefaultValueSql("NOW()");

            //    entity.Property(e => e.Description)
            //        .HasMaxLength(500);

            //    // Связь с лидером
            //    entity.HasOne(e => e.Leader)
            //        .WithOne()
            //        .HasForeignKey<Clan>(e => e.LeaderId)
            //        .OnDelete(DeleteBehavior.Restrict);
            //});
        }
    }
}