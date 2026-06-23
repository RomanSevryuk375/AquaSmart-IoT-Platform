using Contracts.Abstractions;
using Device.Application.Features.Relays.Query.Shared;

namespace Device.Application.Features.Relays.Query.GetAllRelays;

public sealed record GetAllRelaysQuery
    : IQuery<Result<IReadOnlyList<RelayDto>>>
{
    public Guid UserId { get; init; }
    public Guid? ControllerId { get; init; }
    public RelayPurpose? Purpose { get; init; }
    public bool? IsActive { get; init; }
    public bool? IsManual { get; init; }
    public int Skip { get; init; } = 0;
    public int Take { get; init; } = 10;
}
