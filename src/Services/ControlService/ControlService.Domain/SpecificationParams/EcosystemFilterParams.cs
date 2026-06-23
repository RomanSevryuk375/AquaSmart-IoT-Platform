using Contracts.Enums;

namespace Control.Domain.SpecificationParams;

public sealed record EcosystemFilterParams
{
    public Guid? UserId { get; init; }
    public string? Name { get; init; } = string.Empty;
    public Guid? ControllerId { get; init; }
    public EcosystemType? Type { get; init; }
}
