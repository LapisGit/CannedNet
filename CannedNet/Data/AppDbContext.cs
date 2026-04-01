using Microsoft.EntityFrameworkCore;

namespace CannedNet.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<Account> Accounts { get; set; }
    public DbSet<CachedLogin> CachedLogins { get; set; }
    public DbSet<PlayerProgression> PlayerProgressions { get; set; }
    public DbSet<PlayerSetting> PlayerSettings { get; set; }
    public DbSet<AvatarItem> AvatarItems { get; set; }
    public DbSet<PlayerAvatar> PlayerAvatars { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // accounts
        modelBuilder.Entity<Account>(entity =>
        {
            entity.HasKey(e => e.AccountId);
            entity.Property(e => e.AccountId).ValueGeneratedNever();
            entity.Property(e => e.Username).IsRequired();
            entity.Property(e => e.DisplayName).IsRequired();
            entity.ToTable("accounts");
        });

        // cachedlogins
        modelBuilder.Entity<CachedLogin>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).ValueGeneratedOnAdd();
            entity.HasIndex(e => new { e.Platform, e.PlatformID }).IsUnique();
            entity.ToTable("cached_logins");
        });

        // player_progressions
        modelBuilder.Entity<PlayerProgression>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).ValueGeneratedOnAdd();
            entity.HasIndex(e => e.PlayerId).IsUnique();
            entity.ToTable("player_progressions");
        });

        // player_settings_kv (key-value pairs)
        modelBuilder.Entity<PlayerSetting>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).ValueGeneratedOnAdd();
            entity.Property(e => e.PlayerId).IsRequired();
            entity.Property(e => e.Key).IsRequired();
            entity.Property(e => e.Value).IsRequired();
            entity.HasIndex(e => new { e.PlayerId, e.Key }).IsUnique();
            entity.ToTable("player_settings");
        });

        // avatar_items
        modelBuilder.Entity<AvatarItem>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).ValueGeneratedOnAdd();
            entity.Property(e => e.OwnerAccountId).IsRequired();
            entity.Property(e => e.AvatarItemDesc).IsRequired();
            entity.Property(e => e.FriendlyName).IsRequired();
            entity.HasIndex(e => e.OwnerAccountId);
            entity.ToTable("avatar_items");
        });

        // player_avatars
        modelBuilder.Entity<PlayerAvatar>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).ValueGeneratedOnAdd();
            entity.Property(e => e.OwnerAccountId).IsRequired();
            entity.HasIndex(e => e.OwnerAccountId).IsUnique();
            entity.ToTable("player_avatars");
        });
    }
}

