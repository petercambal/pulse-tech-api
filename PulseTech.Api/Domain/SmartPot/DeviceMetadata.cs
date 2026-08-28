namespace PulseTech.Api.Domain.SmartPot;

/// <summary>
/// Tabuľka <c>smart_pot.device_metadata</c>.
/// </summary>
public sealed class DeviceMetadata
{
    public Guid DeviceId { get; set; }

    public string? PlantName { get; set; }
}
