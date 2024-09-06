using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.Extensions.Options;
using SoulDashboard.Options;

namespace SoulDashboard.Identity.Authentication.SoulConnection;

public class SoulConnectionCookieConfigureOptions(IOptionsMonitor<SoulConnectionOptions> optionsMonitor)
    : ConfigureNamedOptions<CookieAuthenticationOptions>(SoulConnectionDefaults.AuthenticationScheme, GetConfigure(optionsMonitor.CurrentValue))
{
    private static Action<CookieAuthenticationOptions> GetConfigure(SoulConnectionOptions options) =>
        cookieOptions => cookieOptions.ClaimsIssuer = options.Authentication.Authority.ToString();
}
