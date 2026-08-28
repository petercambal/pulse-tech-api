using System.Security.Claims;

namespace PulseTech.Api.Common.Auth;

public static class ClaimsPrincipalExtensions
{
    /// <summary>
    /// Reads the authenticated user's id from the <see cref="ClaimTypes.NameIdentifier"/> claim.
    /// Only call this on endpoints protected by <c>RequireAuthorization()</c>.
    /// </summary>
    public static Guid GetUserId(this ClaimsPrincipal principal)
    {
        var raw = principal.FindFirstValue(ClaimTypes.NameIdentifier);

        return Guid.TryParse(raw, out var userId)
            ? userId
            : throw new InvalidOperationException("The authenticated principal does not carry a valid user id.");
    }
}
