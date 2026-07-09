using Contracts.Events.ControllerEvents;
using Device.Domain.Events.ControllerEvents;
using MassTransit;

namespace Device.Application.Handlers;

public sealed class ControllerNotOnlineHandler(
    IPublishEndpoint publishEndpoint,
    IMapper mapper) : INotificationHandler<ControllerNotOnlineDomainEvent>
{
    public async Task Handle(
        ControllerNotOnlineDomainEvent notification,
        CancellationToken cancellationToken)
    {
        await publishEndpoint.Publish(
            mapper.Map<ControllerNotOnlineEvent>(notification), cancellationToken);
    }
}
