namespace PulseTech.Api.Domain.Core;

/// <summary>
/// Tabuľka <c>core.devices</c>. Stĺpec <c>id</c> je <c>char(36)</c> (GUID ako text);
/// rovnaký typ má aj <c>device_id</c> v schéme <c>smart_pot</c>.
/// </summary>
public sealed class Device
{
    public Guid Id { get; set; }

    public Guid OwnerUserId { get; set; }

    public required string Name { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTimeOffset CreatedAt { get; set; }

    public Guid? AppId { get; set; }
}
