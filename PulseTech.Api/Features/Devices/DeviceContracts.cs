namespace PulseTech.Api.Features.Devices;

/// <summary>Response body describing a device (maps <c>core.devices</c>). <see cref="CreatedAt"/> is UTC.</summary>
public sealed record DeviceDto(
    Guid Id,
    Guid OwnerUserId,
    string Name,
    bool IsActive,
    DateTime CreatedAt,
    Guid? AppId);

/// <summary>Request body for <c>POST /api/devices</c>.</summary>
public sealed class CreateDeviceRequest
{
    public Guid OwnerUserId { get; set; }

    public string Name { get; set; } = string.Empty;

    public Guid? AppId { get; set; }

    /// <summary>Defaults to <c>true</c> when omitted.</summary>
    public bool? IsActive { get; set; }
}

/// <summary>Request body for <c>PUT /api/devices/{id}</c> (full replace).</summary>
public sealed class UpdateDeviceRequest
{
    public string Name { get; set; } = string.Empty;

    public bool IsActive { get; set; }

    public Guid? AppId { get; set; }
}
