namespace PulseTech.Api.Features.Auth;

/// <summary>Request body for <c>POST /api/auth/register</c>.</summary>
public sealed record RegisterRequest(string Email, string Password);

/// <summary>Request body for <c>POST /api/auth/login</c>.</summary>
public sealed record LoginRequest(string Email, string Password);

/// <summary>Response body returned by both register and login.</summary>
public sealed record AuthResponse(
    string AccessToken,
    string TokenType,
    DateTime ExpiresAtUtc,
    Guid UserId,
    string Email,
    string Role);
