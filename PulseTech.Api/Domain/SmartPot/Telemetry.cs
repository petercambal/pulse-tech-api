namespace PulseTech.Api.Domain.SmartPot;

/// <summary>
/// Tabuľka <c>smart_pot.telemetry</c> (time-series, hypertable podľa stĺpca <c>time</c>).
/// </summary>
public sealed class Telemetry
{
    public DateTimeOffset Time { get; set; }

    public Guid DeviceId { get; set; }

    public double? SoilMoisture { get; set; }

    public double? LightLux { get; set; }

    public double? AirTempC { get; set; }
}
