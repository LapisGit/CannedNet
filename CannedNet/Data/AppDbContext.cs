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
    public DbSet<SavedOutfit> SavedOutfits { get; set; }
    public DbSet<RoomInstance> RoomInstances { get; set; }
    public DbSet<Room> Rooms { get; set; }
    public DbSet<SubRoom> SubRooms { get; set; }
    public DbSet<LoadScreen> LoadScreens { get; set; }
    public DbSet<PromoImage> PromoImages { get; set; }
    public DbSet<PromoExternalContent> PromoExternalContents { get; set; }
    public DbSet<RoomRole> RoomRoles { get; set; }

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
        
        // saved_outfits
        modelBuilder.Entity<SavedOutfit>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).ValueGeneratedOnAdd();
            entity.Property(e => e.OwnerAccountId).IsRequired();
            entity.HasIndex(e => e.OwnerAccountId).IsUnique();
            entity.ToTable("saved_outfits");
        });
        
        // room_instances
        modelBuilder.Entity<RoomInstance>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).ValueGeneratedOnAdd();
            entity.Property(e => e.OwnerAccountId).IsRequired();
            entity.HasIndex(e => new { e.OwnerAccountId, e.roomId });
            entity.ToTable("room_instances");
        });

        // rooms
        modelBuilder.Entity<Room>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.Name).IsRequired();
            entity.HasIndex(e => e.CreatorAccountId);
            entity.ToTable("rooms");
        });

        // sub_rooms
        modelBuilder.Entity<SubRoom>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).ValueGeneratedOnAdd();
            entity.Property(e => e.RoomId).IsRequired();
            entity.HasIndex(e => e.RoomId);
            entity.ToTable("sub_rooms");
        });

        // load_screens
        modelBuilder.Entity<LoadScreen>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).ValueGeneratedOnAdd();
            entity.Property(e => e.RoomId).IsRequired();
            entity.HasIndex(e => e.RoomId);
            entity.ToTable("load_screens");
        });

        // promo_images
        modelBuilder.Entity<PromoImage>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).ValueGeneratedOnAdd();
            entity.Property(e => e.RoomId).IsRequired();
            entity.HasIndex(e => e.RoomId);
            entity.ToTable("promo_images");
        });

        // promo_external_contents
        modelBuilder.Entity<PromoExternalContent>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).ValueGeneratedOnAdd();
            entity.Property(e => e.RoomId).IsRequired();
            entity.HasIndex(e => e.RoomId);
            entity.ToTable("promo_external_contents");
        });

        // room_roles
        modelBuilder.Entity<RoomRole>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).ValueGeneratedOnAdd();
            entity.Property(e => e.RoomId).IsRequired();
            entity.HasIndex(e => e.RoomId);
            entity.ToTable("room_roles");
        });
    }
}

