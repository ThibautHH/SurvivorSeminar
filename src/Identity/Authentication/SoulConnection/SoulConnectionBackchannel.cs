using Microsoft.Extensions.Options;

namespace SoulDashboard.Identity.Authentication.SoulConnection;

public class SoulConnectionBackchannel : HttpClient
{
    public SoulConnectionBackchannel(IOptionsMonitor<SoulConnectionOptions> optionsMonitor)
    {
        Options = optionsMonitor.CurrentValue;
        BaseAddress = Options.Authority;
        DefaultRequestHeaders.Add(SoulConnectionDefaults.GroupTokenHeaderName, Options.GroupToken);
    }

    public SoulConnectionOptions Options { get; init; }
}
