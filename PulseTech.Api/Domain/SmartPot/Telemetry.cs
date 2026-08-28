namespace PulseTech.Api.Domain.SmartPot;

/// <summary>
/// Maps the <c>smart_pot.telemetry</c> table (time-series, hypertable on the <c>time</c> column).
/// </summary>
public sealed class Telemetry
{
    public DateTimeOffset Time { get; set; }

    public Guid DeviceId { get; set; }

    public double? SoilMoisture { get; set; }

    public double? LightLux { get; set; }

    public double? AirTempC { get; set; }
}
