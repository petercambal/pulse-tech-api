namespace PulseTech.Api.Domain.Core;

/// <summary>
/// Maps the <c>core.applications</c> table.
/// </summary>
public sealed class Application
{
    public Guid Id { get; set; }

    public required string AppCode { get; set; }

    public required string Name { get; set; }

    public string? Description { get; set; }

    public DateTimeOffset CreatedAt { get; set; }
}
