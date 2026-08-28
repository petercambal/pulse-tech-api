using Dapper;
using PulseTech.Api.Common.Database;

namespace PulseTech.Api.Features.Auth;

/// <summary>Row shape read from <c>auth.users</c> for authentication purposes.</summary>
public sealed record AuthUserRecord(
    Guid UserId,
    string Email,
    string PasswordHash,
    string Role,
    bool IsActive);

public sealed class AuthRepository(IDbConnectionFactory connectionFactory)
{
    public async Task<bool> EmailExistsAsync(string email, CancellationToken cancellationToken)
    {
        using var connection = await connectionFactory.OpenConnectionAsync(cancellationToken);

        return await connection.ExecuteScalarAsync<bool>(new CommandDefinition(
            "select exists(select 1 from auth.users where lower(email) = lower(@Email))",
            new { Email = email },
            cancellationToken: cancellationToken));
    }

    public async Task<AuthUserRecord?> GetByEmailAsync(string email, CancellationToken cancellationToken)
    {
        using var connection = await connectionFactory.OpenConnectionAsync(cancellationToken);

        const string sql =
            """
            select user_id       as "UserId",
                   email         as "Email",
                   password_hash as "PasswordHash",
                   role          as "Role",
                   is_active     as "IsActive"
            from auth.users
            where lower(email) = lower(@Email)
            """;

        return await connection.QuerySingleOrDefaultAsync<AuthUserRecord>(new CommandDefinition(
            sql,
            new { Email = email },
            cancellationToken: cancellationToken));
    }

    public async Task<AuthUserRecord> CreateAsync(
        string email,
        string passwordHash,
        string role,
        CancellationToken cancellationToken)
    {
        using var connection = await connectionFactory.OpenConnectionAsync(cancellationToken);

        const string sql =
            """
            insert into auth.users (email, password_hash, role)
            values (@Email, @PasswordHash, @Role)
            returning user_id       as "UserId",
                      email         as "Email",
                      password_hash as "PasswordHash",
                      role          as "Role",
                      is_active     as "IsActive"
            """;

        return await connection.QuerySingleAsync<AuthUserRecord>(new CommandDefinition(
            sql,
            new { Email = email, PasswordHash = passwordHash, Role = role },
            cancellationToken: cancellationToken));
    }
}
