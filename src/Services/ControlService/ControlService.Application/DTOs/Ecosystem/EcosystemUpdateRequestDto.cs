namespace Control.Application.DTOs.Ecosystem;

public sealed record EcosystemUpdateRequestDto
{
    public string Name { get; init; } = string.Empty;
    public double Volume { get; init; }
}
