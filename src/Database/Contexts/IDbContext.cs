namespace SoulDashboard.Database.Contexts;

public interface IDbContext
{
    static abstract string? Schema { get; }
}
