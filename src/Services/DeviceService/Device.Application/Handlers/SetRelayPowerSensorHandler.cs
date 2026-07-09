using Contracts.Events.RelayEvents;
using Device.Domain.Events.RelayEvents;
using MassTransit;

namespace Device.Application.Handlers;

public sealed class SetRelayPowerSensorHandler(
    IPublishEndpoint publishEndpoint,
    IMapper mapper) : INotificationHandler<SetRelayPowerSensorDomainEvent>
{
    public async Task Handle(
        SetRelayPowerSensorDomainEvent notification,
        CancellationToken cancellationToken)
    {
        await publishEndpoint.Publish(
            mapper.Map<SetRelayPowerSensorEvent>(notification), cancellationToken);
    }
}
