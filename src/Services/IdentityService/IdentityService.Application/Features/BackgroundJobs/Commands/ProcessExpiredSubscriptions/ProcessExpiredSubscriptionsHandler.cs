using Contracts.Enums;
using Contracts.Events.UserEvents;
using Contracts.Results;
using IdentityService.Domain.Entities;
using IdentityService.Domain.Interfaces;
using MassTransit;
using MediatR;

namespace IdentityService.Application.Features.BackgroundJobs.Commands.ProcessExpiredSubscriptions;

internal class ProcessExpiredSubscriptionsHandler(
    IUserRepository userRepository,
    IPublishEndpoint publishEndpoint) : IRequestHandler<ProcessExpiredSubscriptionsCommand, Result>
{
    public async Task<Result> Handle(ProcessExpiredSubscriptionsCommand request, CancellationToken cancellationToken)
    {
        IReadOnlyList<User> users = await userRepository.GetWithExpiredSubscriptionAsync(cancellationToken);
        var eventsToPublish = new List<SubscriptionDowngradedEvent>();

        foreach (User user in users)
        {
            user.SetSubscription(Guid
                .Parse(SubscriptionType.Free), SubscriptionType.FreeDuration);

            eventsToPublish.Add(new SubscriptionDowngradedEvent
            {
                UserId = user.Id,
                NewSubscriptionId = user.SubscriptionId,
                OccurredOn = DateTime.UtcNow,
            });
        }

        foreach (SubscriptionDowngradedEvent @event in eventsToPublish)
        {
            await publishEndpoint.Publish(@event, cancellationToken);
        }

        return Result.Success();
    }
}
