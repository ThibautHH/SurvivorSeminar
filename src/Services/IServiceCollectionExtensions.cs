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

        services.AddHttpClient<ISoulConnectionService, DefaultSoulConnectionService>((services, client) =>
        {
            var options = services.GetRequiredService<IOptionsMonitor<SoulConnectionOptions>>().CurrentValue;
            client.BaseAddress = options.Authentication.Authority;
            client.DefaultRequestHeaders.Add(SoulConnectionDefaults.GroupTokenHeaderName, options.GroupToken);
        });

        return services;
    }
}
