namespace PulseTech.Api.Common.Auth;

/// <summary>
/// Strongly-typed binding for the <c>JwtOptions</c> configuration section.
/// </summary>
public sealed class JwtOptions
{
    public const string SectionName = "JwtOptions";

    /// <summary>Symmetric signing key for HMAC-SHA256. Must be at least 32 bytes (256 bits).</summary>
    public string SecretKey { get; set; } = string.Empty;

    public string Issuer { get; set; } = string.Empty;

    public string Audience { get; set; } = string.Empty;

    public int ExpirationInMinutes { get; set; } = 60;
}
