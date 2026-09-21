using AutoMapper;
using BuildingBlocks.IntegrationEvents.Events.Users;
using IdentityService.Domain.Events;
using MassTransit;
using MediatR;

namespace IdentityService.Application.Handlers;

internal sealed class SubscriptionDowngradedEventHandler(
    IPublishEndpoint publishEndpoint,
    IMapper mapper)
    : INotificationHandler<SubscriptionDowngradedDomainEvent>
{
    public async Task Handle(
        SubscriptionDowngradedDomainEvent notification,
        CancellationToken cancellationToken)
    {
        await publishEndpoint.Publish(
            mapper.Map<SubscriptionDowngradedEvent>(notification), cancellationToken);
    }
}
