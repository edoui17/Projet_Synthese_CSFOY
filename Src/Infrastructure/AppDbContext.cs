using Core.Domain;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> p_options) : base(p_options)
    {
    }

    public DbSet<Player> Players { get; set; }
    public DbSet<ResourceItem> ResourceItems { get; set; }
    public DbSet<InventoryEntry> Inventory { get; set; }
    public DbSet<PlayerStats> Stats { get; set; }
    public DbSet<PlayerConfig> PlayerConfigs { get; set; }

    protected override void OnModelCreating(ModelBuilder p_modelBuilder)
    {
        base.OnModelCreating(p_modelBuilder);

        // Player Configuration
        p_modelBuilder.Entity<Player>(entity =>
        {
            entity.ToTable("Players");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasDefaultValueSql("NEWID()");
            entity.Property(e => e.Username).IsRequired().HasMaxLength(100);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETDATE()");
        });

        // ResourceItem Configuration
        p_modelBuilder.Entity<ResourceItem>(entity =>
        {
            entity.ToTable("ResourceItems");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasMaxLength(100);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Type).IsRequired().HasMaxLength(50);
            entity.Property(e => e.IconPath).HasMaxLength(255);
        });

        // Inventory Configuration (Join Table)
        p_modelBuilder.Entity<InventoryEntry>(entity =>
        {
            entity.ToTable("Inventory");
            entity.HasKey(e => new { e.PlayerId, e.ResourceItemId });

            entity.Property(e => e.ResourceItemId).HasMaxLength(100);

            entity.HasOne(e => e.Player)
                .WithMany(p => p.Inventory)
                .HasForeignKey(e => e.PlayerId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.ResourceItem)
                .WithMany(r => r.InventoryEntries)
                .HasForeignKey(e => e.ResourceItemId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Stats Configuration (One-to-One)
        p_modelBuilder.Entity<PlayerStats>(entity =>
        {
            entity.ToTable("Stats");
            entity.HasKey(e => e.PlayerId);

            entity.HasOne(e => e.Player)
                .WithOne(p => p.Stats)
                .HasForeignKey<PlayerStats>(e => e.PlayerId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // PlayerConfig Configuration (One-to-One)
        p_modelBuilder.Entity<PlayerConfig>(entity =>
        {
            entity.ToTable("PlayerConfig");
            entity.HasKey(e => e.PlayerId);

            entity.Property(e => e.Resolution).HasMaxLength(50);

            entity.HasOne(e => e.Player)
                .WithOne(p => p.Config)
                .HasForeignKey<PlayerConfig>(e => e.PlayerId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
