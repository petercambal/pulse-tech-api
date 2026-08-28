using Dapper;
using PulseTech.Api.Common.Database;

namespace PulseTech.Api.Features.Telemetry;

public sealed class TelemetryRepository(IDbConnectionFactory connectionFactory)
{
    public async Task<bool> DeviceBelongsToUserAsync(
        Guid deviceId,
        Guid ownerUserId,
        CancellationToken cancellationToken)
    {
        using var connection = await connectionFactory.OpenConnectionAsync(cancellationToken);

        return await connection.ExecuteScalarAsync<bool>(new CommandDefinition(
            "select exists(select 1 from core.devices where id = @DeviceId::char(36) and owner_user_id = @OwnerUserId)",
            new { DeviceId = deviceId.ToString(), OwnerUserId = ownerUserId },
            cancellationToken: cancellationToken));
    }

    public async Task<IReadOnlyList<TelemetryReadingDto>> GetForDeviceAsync(
        Guid deviceId,
        DateTimeOffset from,
        DateTimeOffset to,
        int limit,
        CancellationToken cancellationToken)
    {
        using var connection = await connectionFactory.OpenConnectionAsync(cancellationToken);

        const string sql =
            """
            select time          as "Time",
                   soil_moisture as "SoilMoisture",
                   light_lux     as "LightLux",
                   air_temp_c    as "AirTempC"
            from smart_pot.telemetry
            where device_id = @DeviceId::char(36)
              and time >= @From
              and time <  @To
            order by time
            limit @Limit
            """;

        var readings = await connection.QueryAsync<TelemetryReadingDto>(new CommandDefinition(
            sql,
            new { DeviceId = deviceId.ToString(), From = from, To = to, Limit = limit },
            cancellationToken: cancellationToken));

        return readings.AsList();
    }
}
