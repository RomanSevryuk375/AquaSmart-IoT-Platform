namespace DTOs;

/// <summary>
/// Lightweight ecosystem info returned from ControlService.
/// </summary>
public sealed record EcosystemDto
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string? Description { get; init; }
}
