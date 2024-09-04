using Microsoft.EntityFrameworkCore;

namespace SoulDashboard.Database.Contexts;

public static class IServiceCollectionExtensions
{
    public static IServiceCollection AddDbContexts(this IServiceCollection services, string connectionString) => services
        .AddDbContext<ApplicationDbContext>(options => options.UseSqlite(connectionString))
        .AddDbContext<IdentityDbContext>(options => options.UseSqlite(connectionString));
}
