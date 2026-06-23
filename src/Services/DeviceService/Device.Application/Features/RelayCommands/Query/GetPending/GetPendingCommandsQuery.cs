using Contracts.Abstractions;

namespace Device.Application.Features.RelayCommands.Query.GetPending;

public sealed record GetPendingCommandsQuery
    : IQuery<Result<IReadOnlyList<RelayCommandDto>>>
{
    public Guid ControllerId { get; init; }
    public string DeviceToken { get; init; } = string.Empty;
}
