using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace PulseTech.Api.Common.Auth;

/// <summary>A signed access token together with its absolute UTC expiration.</summary>
public sealed record GeneratedToken(string AccessToken, DateTime ExpiresAtUtc);

public interface IJwtProvider
{
    GeneratedToken GenerateToken(Guid userId, string email, string role);
}

/// <summary>
/// Issues HMAC-SHA256 signed JWT access tokens carrying the RBAC claims
/// (<see cref="ClaimTypes.NameIdentifier"/>, <see cref="ClaimTypes.Email"/>, <see cref="ClaimTypes.Role"/>).
/// </summary>
public sealed class JwtProvider : IJwtProvider
{
    private readonly JwtOptions _options;
    private readonly SigningCredentials _signingCredentials;

    public JwtProvider(IOptions<JwtOptions> options)
    {
        _options = options.Value;

        if (string.IsNullOrWhiteSpace(_options.SecretKey) || Encoding.UTF8.GetByteCount(_options.SecretKey) < 32)
        {
            throw new InvalidOperationException(
                "JwtOptions:SecretKey must be at least 32 bytes (256 bits) long for HMAC-SHA256.");
        }

        if (_options.ExpirationInMinutes <= 0)
        {
            throw new InvalidOperationException("JwtOptions:ExpirationInMinutes must be greater than zero.");
        }

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_options.SecretKey));
        _signingCredentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
    }

    public GeneratedToken GenerateToken(Guid userId, string email, string role)
    {
        var nowUtc = DateTime.UtcNow;
        var expiresAtUtc = nowUtc.AddMinutes(_options.ExpirationInMinutes);

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
            new Claim(ClaimTypes.Email, email),
            new Claim(ClaimTypes.Role, role),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new Claim(
                JwtRegisteredClaimNames.Iat,
                new DateTimeOffset(nowUtc).ToUnixTimeSeconds().ToString(),
                ClaimValueTypes.Integer64),
        };

        var token = new JwtSecurityToken(
            issuer: _options.Issuer,
            audience: _options.Audience,
            claims: claims,
            notBefore: nowUtc,
            expires: expiresAtUtc,
            signingCredentials: _signingCredentials);

        // Keep the claim types exactly as declared above (no short-name remapping),
        // so the API can validate them against ClaimTypes.* on the way in.
        var handler = new JwtSecurityTokenHandler();
        handler.OutboundClaimTypeMap.Clear();

        return new GeneratedToken(handler.WriteToken(token), expiresAtUtc);
    }
}
