using Dapper;
using PulseTech.Api.Common.Database;

namespace PulseTech.Api.Features.Devices;

/// <summary>
/// All read/write operations are scoped to a single owning user
/// (<c>core.devices.owner_user_id</c>) so callers can only ever touch their own devices.
/// </summary>
public sealed class DeviceRepository(IDbConnectionFactory connectionFactory)
{
    private const string DeviceColumns =
        """
        id::uuid      as "Id",
        owner_user_id as "OwnerUserId",
        name          as "Name",
        is_active     as "IsActive",
        created_at    as "CreatedAt",
        app_id        as "AppId"
        """;

    public async Task<IReadOnlyList<DeviceDto>> ListAsync(
        Guid ownerUserId,
        Guid? appId,
        bool? isActive,
        int limit,
        int offset,
        CancellationToken cancellationToken)
    {
        using var connection = await connectionFactory.OpenConnectionAsync(cancellationToken);

        var sql =
            $"""
            select {DeviceColumns}
            from core.devices
            where owner_user_id = @OwnerUserId
              and (@AppId is null or app_id = @AppId)
              and (@IsActive is null or is_active = @IsActive)
            order by created_at desc
            limit @Limit offset @Offset
            """;

        var devices = await connection.QueryAsync<DeviceDto>(new CommandDefinition(
            sql,
            new { OwnerUserId = ownerUserId, AppId = appId, IsActive = isActive, Limit = limit, Offset = offset },
            cancellationToken: cancellationToken));

        return devices.AsList();
    }

    public async Task<DeviceDto?> GetByIdAsync(Guid id, Guid ownerUserId, CancellationToken cancellationToken)
    {
        using var connection = await connectionFactory.OpenConnectionAsync(cancellationToken);

        return await connection.QuerySingleOrDefaultAsync<DeviceDto>(new CommandDefinition(
            $"select {DeviceColumns} from core.devices where id = @Id::char(36) and owner_user_id = @OwnerUserId",
            new { Id = id.ToString(), OwnerUserId = ownerUserId },
            cancellationToken: cancellationToken));
    }

    public async Task<DeviceDto> CreateAsync(
        Guid ownerUserId,
        CreateDeviceRequest request,
        CancellationToken cancellationToken)
    {
        using var connection = await connectionFactory.OpenConnectionAsync(cancellationToken);

        var sql =
            $"""
            insert into core.devices (owner_user_id, name, is_active, app_id)
            values (@OwnerUserId, @Name, @IsActive, @AppId)
            returning {DeviceColumns}
            """;

        return await connection.QuerySingleAsync<DeviceDto>(new CommandDefinition(
            sql,
            new
            {
                OwnerUserId = ownerUserId,
                request.Name,
                IsActive = request.IsActive ?? true,
                request.AppId,
            },
            cancellationToken: cancellationToken));
    }

    public async Task<DeviceDto?> UpdateAsync(
        Guid id,
        Guid ownerUserId,
        UpdateDeviceRequest request,
        CancellationToken cancellationToken)
    {
        using var connection = await connectionFactory.OpenConnectionAsync(cancellationToken);

        var sql =
            $"""
            update core.devices
            set name = @Name,
                is_active = @IsActive,
                app_id = @AppId
            where id = @Id::char(36) and owner_user_id = @OwnerUserId
            returning {DeviceColumns}
            """;

        return await connection.QuerySingleOrDefaultAsync<DeviceDto>(new CommandDefinition(
            sql,
            new { Id = id.ToString(), OwnerUserId = ownerUserId, request.Name, request.IsActive, request.AppId },
            cancellationToken: cancellationToken));
    }

    public async Task<bool> DeleteAsync(Guid id, Guid ownerUserId, CancellationToken cancellationToken)
    {
        using var connection = await connectionFactory.OpenConnectionAsync(cancellationToken);

        var affected = await connection.ExecuteAsync(new CommandDefinition(
            "delete from core.devices where id = @Id::char(36) and owner_user_id = @OwnerUserId",
            new { Id = id.ToString(), OwnerUserId = ownerUserId },
            cancellationToken: cancellationToken));

        return affected > 0;
    }

    public async Task<bool> ApplicationExistsAsync(Guid appId, CancellationToken cancellationToken)
    {
        using var connection = await connectionFactory.OpenConnectionAsync(cancellationToken);

        return await connection.ExecuteScalarAsync<bool>(new CommandDefinition(
            "select exists(select 1 from core.applications where id = @AppId)",
            new { AppId = appId },
            cancellationToken: cancellationToken));
    }
}
