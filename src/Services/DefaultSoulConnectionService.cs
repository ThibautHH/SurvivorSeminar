using System.Text.Json.Serialization;
using Microsoft.Extensions.Options;
using SoulDashboard.Identity.Authentication.SoulConnection;
using SoulDashboard.Services.Data;

namespace SoulDashboard.Services;

public class DefaultSoulConnectionService(HttpClient backchannel,
    IOptionsMonitor<SoulConnectionOptions> optionsMonitor,
    ILogger<ISoulConnectionService> logger)
    : ISoulConnectionService
{
    private sealed record LoginResponse([property: JsonPropertyName("access_token")] string AccessToken, string? Detail);

    public async Task<LoginResult> LoginAsync(string email, string password, CancellationToken cancellationToken = default)
    {
        var message = new HttpRequestMessage(HttpMethod.Post,
            new Uri(optionsMonitor.CurrentValue.Authority, optionsMonitor.CurrentValue.LoginEndpoint))
        {
            Content = JsonContent.Create(new { Email = email, Password = password })
        };

        var response = await backchannel.SendAsync(message, cancellationToken);

        LoginResponse loginResponse;

        try
        {
            loginResponse = await response.Content.ReadFromJsonAsync<LoginResponse>(cancellationToken)
                ?? throw new Exception();
        } catch (Exception)
        {
            return new(null, "Couldn't process the response from the API.", false);
        }

        return response.IsSuccessStatusCode
            ? new(loginResponse.AccessToken, null, true)
            : new(null, loginResponse.Detail, false);
    }
}
