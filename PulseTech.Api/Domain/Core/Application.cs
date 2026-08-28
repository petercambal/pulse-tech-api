namespace PulseTech.Api.Domain.Core;

/// <summary>
/// Tabuľka <c>core.applications</c>.
/// </summary>
public sealed class Application
{
    public Guid Id { get; set; }

    public required string AppCode { get; set; }

    public required string Name { get; set; }

    public string? Description { get; set; }

    public DateTimeOffset CreatedAt { get; set; }
}
