using Contracts.Events.RelayEvents;
using Device.Domain.Events.RelayEvents;
using MassTransit;
using MediatR;

namespace Device.Application.Handlers;

public sealed class SetRelayPowerSensorHandler(IPublishEndpoint publishEndpoint)
    : INotificationHandler<SetRelayPowerSensorDomainEvent>
{
    public async Task Handle(
        SetRelayPowerSensorDomainEvent notification, 
        CancellationToken cancellationToken)
    {
        await publishEndpoint.Publish(new SetRelayPowerSensorEvent
        {
            RelayId = notification.RelayId,
            PowerSensorId = notification.PowerSensorId,
        }, cancellationToken);
    }
}
