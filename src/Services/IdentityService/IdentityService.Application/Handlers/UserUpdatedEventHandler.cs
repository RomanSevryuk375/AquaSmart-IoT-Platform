using AutoMapper;
using BuildingBlocks.IntegrationEvents.Events.Users;
using IdentityService.Domain.Events;
using MassTransit;
using MediatR;

namespace IdentityService.Application.Handlers;

internal sealed class UserUpdatedEventHandler(
    IPublishEndpoint publishEndpoint,
    IMapper mapper) : INotificationHandler<UserUpdatedDomainEvent>
{
    public async Task Handle(UserUpdatedDomainEvent notification, CancellationToken cancellationToken)
    {
        await publishEndpoint.Publish(
            mapper.Map<UserUpdatedEvent>(notification), cancellationToken);
    }
}
