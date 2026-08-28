namespace PulseTech.Api.Features.Telemetry;

/// <summary>
/// Request body for <c>POST /api/devices/{deviceId}/telemetry/query</c>.
/// </summary>
public sealed class GetDeviceTelemetryRequest
{
    /// <summary>Start of the period, inclusive. Defaults to <see cref="To"/> minus 24 hours.</summary>
    public DateTimeOffset? From { get; set; }

    /// <summary>End of the period, exclusive. Defaults to the current UTC time.</summary>
    public DateTimeOffset? To { get; set; }

    /// <summary>Maximum number of readings to return. Clamped to 1..50000, defaults to 10000.</summary>
    public int? Limit { get; set; }
}

/// <summary>A single telemetry sample. <see cref="Time"/> is UTC.</summary>
public sealed record TelemetryReadingDto(
    DateTime Time,
    double? SoilMoisture,
    double? LightLux,
    double? AirTempC);

/// <summary>Response body for the device telemetry endpoint.</summary>
public sealed record DeviceTelemetryResponse(
    Guid DeviceId,
    DateTimeOffset From,
    DateTimeOffset To,
    int Count,
    IReadOnlyList<TelemetryReadingDto> Readings);
