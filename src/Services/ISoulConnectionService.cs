using SoulDashboard.Services.Data;

namespace SoulDashboard.Services;

public interface ISoulConnectionService
{
    public Task<LoginResult> LoginAsync(string username, string password, CancellationToken cancellationToken = default);
}
