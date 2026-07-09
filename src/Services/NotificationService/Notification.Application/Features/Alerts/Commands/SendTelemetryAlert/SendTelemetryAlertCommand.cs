using Contracts.Abstractions;

namespace Notification.Application.Features.Alerts.Commands.SendTelemetryAlert;

public sealed record SendTelemetryAlertCommand : ICommand
{
    public Guid UserId { get; init; }
    public Guid EcosystemId { get; init; }
    public Guid SensorId { get; init; }
    public double Value { get; init; }
    public DateTime RecordedAt { get; init; }
    public Guid RelayId { get; init; }
    public bool RelayState { get; init; }
}
