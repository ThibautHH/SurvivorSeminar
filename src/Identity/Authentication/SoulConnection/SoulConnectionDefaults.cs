using Microsoft.AspNetCore.Authentication.Cookies;

namespace SoulDashboard.Identity.Authentication.SoulConnection;

public static class SoulConnectionDefaults
{
    public const string AuthenticationScheme = "SoulConnection";

    public const string GroupTokenHeaderName = "X-Group-Authorization";

    public const string TokenClaimType = Authority + LoginEndpoint;

    public const string Authority = "https://soul-connection.fr";

    public const string LoginEndpoint = "/api/employees/login";

    public const string AuthenticationEndpoint = "/api/employees/me";

    public static void Configure(CookieAuthenticationOptions options)
    {
        options.Cookie.Name = AuthenticationScheme;
        options.ClaimsIssuer = Authority;
        options.LoginPath = "/Account/SoulLogin";
    }
}
