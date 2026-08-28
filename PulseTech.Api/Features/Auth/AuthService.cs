using PulseTech.Api.Common.Auth;

namespace PulseTech.Api.Features.Auth;

public enum RegisterStatus
{
    Success,
    EmailAlreadyRegistered,
}

public enum LoginStatus
{
    Success,
    InvalidCredentials,
    AccountDisabled,
}

public sealed record RegisterResult(RegisterStatus Status, AuthResponse? Response);

public sealed record LoginResult(LoginStatus Status, AuthResponse? Response);

/// <summary>
/// Registration and login logic: BCrypt password hashing/verification and JWT issuance.
/// </summary>
public sealed class AuthService(AuthRepository repository, IJwtProvider jwtProvider)
{
    /// <summary>New self-service accounts always start as plain users; elevation is a separate admin action.</summary>
    private const string DefaultRole = "User";

    private const int BcryptWorkFactor = 12;

    public async Task<RegisterResult> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken)
    {
        var email = NormalizeEmail(request.Email);

        if (await repository.EmailExistsAsync(email, cancellationToken))
        {
            return new RegisterResult(RegisterStatus.EmailAlreadyRegistered, null);
        }

        var passwordHash = BCrypt.Net.BCrypt.HashPassword(request.Password, BcryptWorkFactor);

        var user = await repository.CreateAsync(email, passwordHash, DefaultRole, cancellationToken);

        return new RegisterResult(RegisterStatus.Success, BuildResponse(user));
    }

    public async Task<LoginResult> LoginAsync(LoginRequest request, CancellationToken cancellationToken)
    {
        var email = NormalizeEmail(request.Email);

        var user = await repository.GetByEmailAsync(email, cancellationToken);

        if (user is null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
        {
            return new LoginResult(LoginStatus.InvalidCredentials, null);
        }

        if (!user.IsActive)
        {
            return new LoginResult(LoginStatus.AccountDisabled, null);
        }

        return new LoginResult(LoginStatus.Success, BuildResponse(user));
    }

    private AuthResponse BuildResponse(AuthUserRecord user)
    {
        var token = jwtProvider.GenerateToken(user.UserId, user.Email, user.Role);

        return new AuthResponse(
            token.AccessToken,
            TokenType: "Bearer",
            token.ExpiresAtUtc,
            user.UserId,
            user.Email,
            user.Role);
    }

    private static string NormalizeEmail(string email) => email.Trim().ToLowerInvariant();
}
