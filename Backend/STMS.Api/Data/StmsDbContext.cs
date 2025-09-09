using Microsoft.EntityFrameworkCore;
using STMS.Api.Models;

namespace STMS.Api.Data
{
    public class StmsDbContext : DbContext
    {
        public StmsDbContext(DbContextOptions<StmsDbContext> options) : base(options) { }

        public DbSet<Admin> Admins => Set<Admin>();
        public DbSet<Tournament> Tournaments => Set<Tournament>();
        public DbSet<University> Universities => Set<University>();
        public DbSet<Player> Players => Set<Player>();
        public DbSet<Timing> Timings => Set<Timing>();
        public DbSet<PlayerEvent> PlayerEvents => Set<PlayerEvent>();


        protected override void OnModelCreating(ModelBuilder model)
        {
            base.OnModelCreating(model);

            model.Entity<Admin>(e =>
            {
                e.ToTable("Admins");
                e.HasKey(x => x.Id);
                e.Property(x => x.Email).IsRequired().HasMaxLength(255);
                e.HasIndex(x => x.Email).IsUnique();
                e.Property(x => x.PasswordHash).IsRequired().HasMaxLength(255);
                e.Property(x => x.CreatedAt).HasColumnType("datetime").HasDefaultValueSql("CURRENT_TIMESTAMP");
            });

            model.Entity<Tournament>(e =>
            {
                e.ToTable("Tournaments");
                e.HasKey(x => x.Id);
                e.Property(x => x.Name).IsRequired().HasMaxLength(120);
                e.Property(x => x.Venue).IsRequired().HasMaxLength(120);
                e.Property(x => x.Date).HasColumnType("date").IsRequired();
                e.Property(x => x.CreatedAt).HasColumnType("datetime").HasDefaultValueSql("CURRENT_TIMESTAMP");
                e.HasIndex(x => new { x.Date, x.Venue });
            });

            model.Entity<University>(e =>
            {
                e.ToTable("Universities");
                e.HasKey(x => x.Id);
                e.Property(x => x.Name).IsRequired().HasMaxLength(160);
                e.Property(x => x.CreatedAt).HasColumnType("datetime").HasDefaultValueSql("CURRENT_TIMESTAMP");
                e.HasOne(x => x.Tournament).WithMany().HasForeignKey(x => x.TournamentId).OnDelete(DeleteBehavior.Cascade);
                e.HasIndex(x => new { x.TournamentId, x.Name });
            });

            model.Entity<Player>(e =>
            {
                e.ToTable("Players");
                e.HasKey(x => x.Id);
                e.Property(x => x.Name).IsRequired().HasMaxLength(160);
                e.Property(x => x.CreatedAt).HasColumnType("datetime").HasDefaultValueSql("CURRENT_TIMESTAMP");
                e.HasOne(x => x.University).WithMany().HasForeignKey(x => x.UniversityId).OnDelete(DeleteBehavior.Cascade);
                e.HasIndex(x => new { x.UniversityId, x.Name });
            });

            model.Entity<Timing>(e =>
            {
                e.ToTable("Timings");
                e.HasKey(x => x.Id);
                e.Property(x => x.Event).IsRequired().HasMaxLength(120);
                e.Property(x => x.TimeMs).IsRequired();
                e.Property(x => x.CreatedAt).HasColumnType("datetime").HasDefaultValueSql("CURRENT_TIMESTAMP");
                e.HasOne(x => x.Player).WithMany().HasForeignKey(x => x.PlayerId).OnDelete(DeleteBehavior.Cascade);
                e.HasIndex(x => new { x.PlayerId, x.Event });
            });

            model.Entity<PlayerEvent>(e =>
            {
                e.ToTable("PlayerEvents");
                e.HasKey(x => x.Id);
                e.Property(x => x.Event).IsRequired().HasMaxLength(120);
                e.Property(x => x.CreatedAt).HasColumnType("datetime")
                    .HasDefaultValueSql("CURRENT_TIMESTAMP");
                e.HasOne(x => x.Player)
                    .WithMany()
                    .HasForeignKey(x => x.PlayerId)
                    .OnDelete(DeleteBehavior.Cascade);

                // One player should not be registered twice for the same event
                e.HasIndex(x => new { x.PlayerId, x.Event }).IsUnique();
            });
        }
    }
}
