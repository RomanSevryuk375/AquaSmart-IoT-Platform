using Contracts.Enums;
using MediatR;

namespace Control.Application.CQRS.Ecosystem.Queries.GetAllEcosystems;

public sealed record GetAllEcosystemsQuery : IRequest<IReadOnlyList<EcosystemDto>>
{
    public Guid? UserId { get; init; }
    public string? Name { get; init; } = string.Empty;
    public Guid? ControllerId { get; init; }
    public EcosystemTypeEnum? Type { get; init; }
    public int Skip { get; init; } = 0;
    public int Take { get; init; } = 10;
}
