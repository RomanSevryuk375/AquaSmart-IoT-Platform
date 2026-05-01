using Contracts.Enums;

namespace Control.Application.DTOs.Ecosystem;

public sealed record EcosystemFilterDto
{
    public Guid? UserId { get; init; }
    public string? Name { get; init; } = string.Empty;
    public Guid? ControllerId { get; init; }
    public EcosystemTypeEnum? Type { get; init; }
}
