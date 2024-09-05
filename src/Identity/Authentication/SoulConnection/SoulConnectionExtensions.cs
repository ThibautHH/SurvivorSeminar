using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.Extensions.Options;
using SoulDashboard.Services;

namespace SoulDashboard.Identity.Authentication.SoulConnection;

public static class SoulConnectionExtensions
{
    public static IServiceCollection AddSoulConnection(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<SoulConnectionOptions>(configuration.GetRequiredSection(nameof(Authentication)))
            .AddSingleton<IConfigureOptions<CookieAuthenticationOptions>, SoulConnectionCookieConfigureOptions>();

        services.AddHttpClient<ISoulConnectionService>((services, client) =>
        {
            var options = services.GetRequiredService<IOptionsMonitor<SoulConnectionOptions>>().CurrentValue;
            client.BaseAddress = options.Authority;
            client.DefaultRequestHeaders.Add(SoulConnectionDefaults.GroupTokenHeaderName, options.GroupToken);
        });

        return services;
    }
}
