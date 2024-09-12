using SoulDashboard.Identity.Authentication.SoulConnection;

namespace SoulDashboard.Options;

public class SoulConnectionOptions
{
    public string GroupToken { get; set; } = string.Empty;

    public AuthenticationOptions Authentication { get; set; } = new();

    public SynchronizationOptions Synchronization { get; set; } = new();

    public sealed class AuthenticationOptions
    {
        public Uri Authority { get; set; } = new(SoulConnectionDefaults.Authority);

        public string LoginEndpoint { get; set; } = SoulConnectionDefaults.LoginEndpoint;
    }

    public sealed class SynchronizationOptions
    {
        public int Interval { get; set; } = 30;

        public bool UpdateExisitingRecords { get; set; }

        public Uri Host { get; set; } = new(SoulConnectionDefaults.Authority);

        public CredentialsOptions Credentials { get; set; } = new();

        public sealed class CredentialsOptions
        {
            public string Username { get; set; } = string.Empty;

            public string Password { get; set; } = string.Empty;
        }
    }
}
