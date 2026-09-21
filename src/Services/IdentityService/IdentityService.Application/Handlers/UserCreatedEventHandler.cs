using AutoMapper;
using BuildingBlocks.IntegrationEvents.Events.Users;
using IdentityService.Domain.Events;
using MassTransit;
using MediatR;

namespace IdentityService.Application.Handlers;

internal sealed class UserCreatedEventHandler(
    IPublishEndpoint publishEndpoint,
    IMapper mapper) : INotificationHandler<UserCreatedDomainEvent>
{
    public async Task Handle(UserCreatedDomainEvent notification, CancellationToken cancellationToken)
    {
        await publishEndpoint.Publish(
            mapper.Map<UserCreatedEvent>(notification), cancellationToken);
    }
}
