using BuildingBlocks.IntegrationEvents.Events.Relays;
using Device.Domain.Events.RelayEvents;
using MassTransit;

namespace Device.Application.Handlers;

internal sealed class RemoveRelayPowerSensorHandler(
    IPublishEndpoint publishEndpoint,
    IMapper mapper)
    : INotificationHandler<RemoveRelayPowerSensorDomainEvent>
{
    public async Task Handle(
        RemoveRelayPowerSensorDomainEvent notification,
        CancellationToken cancellationToken)
    {
        await publishEndpoint.Publish(
            mapper.Map<RemoveRelayPowerSensorEvent>(notification), cancellationToken);
    }
}
