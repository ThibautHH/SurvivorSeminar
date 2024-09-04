using Microsoft.EntityFrameworkCore;
using SoulDashboard.Data;

namespace SoulDashboard.Database.Contexts;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : DbContext(options), IDbContext
{
    public static string? Schema => null;

    public DbSet<Cloth> Clothes { get; set; } = default!;

    public DbSet<Customer> Customers { get; set; } = default!;

    public DbSet<Employee> Employees { get; set; } = default!;

    public DbSet<Encounter> Encounters { get; set; } = default!;

    public DbSet<Event> Events { get; set; } = default!;

    public DbSet<Payment> Payments { get; set; } = default!;

    public DbSet<Tip> Tips { get; set; } = default!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema(Schema);

        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Employee>(b =>
        {
            b.HasIndex(e => e.NormalizedEmail)
                .HasDatabaseName("EmailIndex");

            b.HasIndex(e => e.NormalizedUserName)
                .IsUnique()
                .HasDatabaseName("UserNameIndex");

            b.ToTable("Users", IdentityDbContext.Schema);
        });
    }
}
