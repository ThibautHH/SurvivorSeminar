using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.Extensions.Options;

namespace SoulDashboard.Identity.Authentication.SoulConnection;

public class SoulConnectionCookieConfigureOptions(IOptionsMonitor<SoulConnectionOptions> optionsMonitor)
    : ConfigureNamedOptions<CookieAuthenticationOptions>(SoulConnectionDefaults.AuthenticationScheme, GetConfigure(optionsMonitor.CurrentValue))
{
    private static Action<CookieAuthenticationOptions> GetConfigure(SoulConnectionOptions options) =>
        cookieOptions => cookieOptions.ClaimsIssuer = options.Authority.ToString();
}
