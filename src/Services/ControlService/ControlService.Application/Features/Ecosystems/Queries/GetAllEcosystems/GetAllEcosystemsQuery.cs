using Contracts.Abstractions;
using Contracts.Enums;

namespace Control.Application.Features.Ecosystems.Queries.GetAllEcosystems;

public sealed record GetAllEcosystemsQuery
    : IQuery<IReadOnlyList<EcosystemDto>>
{
    public Guid? UserId { get; init; }
    public string? Name { get; init; } = string.Empty;
    public Guid? ControllerId { get; init; }
    public EcosystemType? Type { get; init; }
    public int Skip { get; init; } = 0;
    public int Take { get; init; } = 10;
}
