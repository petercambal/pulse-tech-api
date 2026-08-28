namespace PulseTech.Api.Domain.SmartPot;

/// <summary>
/// Maps the <c>smart_pot.device_metadata</c> table.
/// </summary>
public sealed class DeviceMetadata
{
    public Guid DeviceId { get; set; }

    public string? PlantName { get; set; }
}
