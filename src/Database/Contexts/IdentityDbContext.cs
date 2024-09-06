using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using SoulDashboard.Data;

namespace SoulDashboard.Database.Contexts;

public class IdentityDbContext(DbContextOptions<IdentityDbContext> options)
    : IdentityDbContext<Employee, IdentityRole<int>, int>(options), IDbContext
{
    public static string? Schema => "identity";

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema(Schema);

        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Employee>().ToTable("Users");
        modelBuilder.Entity<IdentityRole<int>>().ToTable("Roles");
        modelBuilder.Entity<IdentityRoleClaim<int>>().ToTable("RoleClaims");
        modelBuilder.Entity<IdentityUserRole<int>>().ToTable("UserRoles");
        modelBuilder.Entity<IdentityUserClaim<int>>().ToTable("UserClaims");
        modelBuilder.Entity<IdentityUserLogin<int>>().ToTable("UserLogins");
        modelBuilder.Entity<IdentityUserToken<int>>().ToTable("UserTokens");

        modelBuilder.Ignore<Customer>();
    }
}
