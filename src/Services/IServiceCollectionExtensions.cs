using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.Extensions.Options;
using SoulDashboard.Identity.Authentication.SoulConnection;
using SoulDashboard.Options;

namespace SoulDashboard.Services;

public static class IServiceCollectionExtensions
{
    public static IServiceCollection AddSoulConnection(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<SoulConnectionOptions>(configuration)
            .AddSingleton<IConfigureOptions<CookieAuthenticationOptions>, SoulConnectionCookieConfigureOptions>();

        services.AddHttpClient<ISoulConnectionService, DefaultSoulConnectionService>(static (services, client) =>
        {
            var options = services.GetRequiredService<IOptionsMonitor<SoulConnectionOptions>>().CurrentValue;
            client.BaseAddress = options.Authentication.Authority;
            client.DefaultRequestHeaders.Add(SoulConnectionDefaults.GroupTokenHeaderName, options.GroupToken);
        });

        services.AddHttpClient<SoulConnectionDataService>(static (services, client) =>
        {
            var options = services.GetRequiredService<IOptionsMonitor<SoulConnectionOptions>>().CurrentValue;
            client.BaseAddress = options.Synchronization.Host;
            client.DefaultRequestHeaders.Add(SoulConnectionDefaults.GroupTokenHeaderName, options.GroupToken);
            var credentials = options.Synchronization.Credentials;
            var result = services.GetRequiredService<ISoulConnectionService>().LoginAsync(credentials.Username, credentials.Password).Result;
            if (!result.Succeded)
                throw new InvalidOperationException("Invalid credentials for Soul Connection synchronization.");
            client.DefaultRequestHeaders.Authorization = new("Bearer", result.AccessToken);
        });

        services.AddHostedService<SoulConnectionSynchronizationService>();

        services.Configure<HostOptions>(static options => options.BackgroundServiceExceptionBehavior = BackgroundServiceExceptionBehavior.Ignore);

        return services;
    }
}
