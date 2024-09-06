using SoulDashboard.Identity.Authentication.SoulConnection;

namespace SoulDashboard.Options;

public class SoulConnectionOptions
{
    public string GroupToken { get; set; } = string.Empty;

    public AuthenticationOptions Authentication { get; set; } = new();

    public sealed class AuthenticationOptions
    {
        public Uri Authority { get; set; } = new(SoulConnectionDefaults.Authority);

        public string LoginEndpoint { get; set; } = SoulConnectionDefaults.LoginEndpoint;
    }
}
