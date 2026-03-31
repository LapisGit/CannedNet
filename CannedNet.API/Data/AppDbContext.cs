using Microsoft.EntityFrameworkCore;

namespace CannedNet.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<Account> Accounts { get; set; }
    public DbSet<CachedLogin> CachedLogins { get; set; }

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
    }
}

