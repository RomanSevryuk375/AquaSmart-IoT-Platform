using BuildingBlocks.Domain.Abstractions;
using BuildingBlocks.Domain.Results;

namespace Device.Application.Features.RelayCommands.Query.GetPending;

public sealed record GetPendingCommandsQuery
    : IQuery<Result<IReadOnlyList<RelayCommandDto>>>
{
    public Guid ControllerId { get; init; }
}
