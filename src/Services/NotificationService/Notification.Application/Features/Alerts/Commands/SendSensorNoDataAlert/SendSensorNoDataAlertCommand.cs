using Contracts.Abstractions;

namespace Notification.Application.Features.Alerts.Commands.SendSensorNoDataAlert;

public sealed record SendSensorNoDataAlertCommand : ICommand
{
    public Guid UserId { get; init; }
    public Guid EcosystemId { get; init; }
    public Guid SensorId { get; init; }
    public DateTime LastSeenAt { get; init; }
}
