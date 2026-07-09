using Contracts.Abstractions;
using Contracts.Enums;

namespace Control.Application.Features.Relays.Commands.SyncRelayUpdated;

public sealed record SyncRelayUpdatedCommand : ICommand
{
    public Guid RelayId { get; init; }
    public Guid ControllerId { get; init; }
    public Guid? PowerSensorId { get; init; }
    public string Name { get; init; } = string.Empty;
    public RelayPurpose Purpose { get; init; }
    public bool IsManual { get; init; }
    public bool IsActive { get; init; }
    public DateTime CreatedAt { get; init; }
}
