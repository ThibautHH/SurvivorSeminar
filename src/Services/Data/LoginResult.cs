using System.Diagnostics.CodeAnalysis;

namespace SoulDashboard.Services.Data;

public record LoginResult(string? AccessToken, string? ErrorMessage,
    [property: MemberNotNullWhen(true, nameof(LoginResult.AccessToken))] bool Succeded);
