using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.Extensions.Options;

namespace SoulDashboard.Identity.Authentication.SoulConnection;

public static class SoulConnectionExtensions
{
    public static IServiceCollection AddSoulConnection(this IServiceCollection services, IConfiguration? configuration = null) =>
        (configuration is not null ? services.Configure<SoulConnectionOptions>(configuration) : services)
            .AddSingleton<IConfigureOptions<CookieAuthenticationOptions>, SoulConnectionCookieConfigureOptions>()
            .AddScoped<SoulConnectionBackchannel>();
}
