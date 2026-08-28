namespace PulseTech.Api.Domain.Core;

/// <summary>
/// Maps the <c>core.devices</c> table. The <c>id</c> column is <c>char(36)</c> (GUID as text);
/// the <c>device_id</c> columns in the <c>smart_pot</c> schema use the same type.
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
