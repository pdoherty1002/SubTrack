using Microsoft.EntityFrameworkCore;
using SubTrack.Domain.Entities;

namespace SubTrack.Infrastructure.Persistence;

/// <summary>
/// The EF Core database context for the whole application — the single point of contact
/// between the C# entities and the underlying PostgreSQL tables.
/// </summary>
public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    /// <summary>The Users table.</summary>
    public DbSet<ApplicationUser> Users => Set<ApplicationUser>();

    /// <summary>The Subscriptions table.</summary>
    public DbSet<Subscription> Subscriptions => Set<Subscription>();

    /// <summary>
    /// Configures indexes, uniqueness constraints, and relationship/delete behavior
    /// using the Fluent API — kept here rather than as attributes on the entities, so
    /// the Domain project stays free of any EF Core dependency.
    /// </summary>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ApplicationUser>(entity =>
        {
            // No two users can register with the same email.
            entity.HasIndex(u => u.Email).IsUnique();
        });

        modelBuilder.Entity<Subscription>(entity =>
        {
            entity.Property(s => s.Cost).HasPrecision(18, 2);

            // Deleting a user deletes their subscriptions — a subscription pointing at
            // a nonexistent user is meaningless.
            entity.HasOne(s => s.User)
                  .WithMany(u => u.Subscriptions)
                  .HasForeignKey(s => s.UserId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        base.OnModelCreating(modelBuilder);
    }
}
