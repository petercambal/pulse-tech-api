namespace PulseTech.Api.Domain.Auth;

/// <summary>
/// Tabuľka <c>auth.users</c>.
/// </summary>
public sealed class User
{
    public Guid UserId { get; set; }

    public required string Email { get; set; }

    public required string PasswordHash { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTimeOffset CreatedAt { get; set; }
}
