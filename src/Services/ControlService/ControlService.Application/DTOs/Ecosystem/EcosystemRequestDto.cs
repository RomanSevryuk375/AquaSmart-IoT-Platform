using Contracts.Enums;

namespace Control.Application.DTOs.Ecosystem;

public sealed record EcosystemRequestDto
{
    public EcosystemTypeEnum Type { get; init; }
    public string Name { get; init; } = string.Empty;
    public double? Volume { get; init; }
    public Guid ControllerId { get; init; }
}
