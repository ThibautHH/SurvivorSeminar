namespace SoulDashboard.Identity.Authentication.SoulConnection;

public class SoulConnectionOptions
{
    public string GroupToken { get; set; } = string.Empty;

    public Uri Authority { get; set; } = new(SoulConnectionDefaults.Authority);

    public string LoginEndpoint { get; set; } = SoulConnectionDefaults.LoginEndpoint;

    public string AuthenticationEndpoint { get; set; } = SoulConnectionDefaults.AuthenticationEndpoint;
}
