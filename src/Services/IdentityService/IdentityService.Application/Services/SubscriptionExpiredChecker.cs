using Contracts.Enums;
using Contracts.Events.UserEvents;
using IdentityService.Application.Interfaces;
using IdentityService.Domain.Entities;
using IdentityService.Domain.Interfaces;
using MassTransit;

namespace IdentityService.Application.Services;

public class SubscriptionExpiredChecker(
    IUserRepository userRepository,
    IUnitOfWork unitOfWork,
    IPublishEndpoint publishEndpoint) : ISubscriptionExpiredChecker
{
    public async Task CheckAsync(CancellationToken cancellationToken)
    {
        IReadOnlyList<User> users = await userRepository
            .GetWithExpiredSubscriptionAsync(cancellationToken);

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

        await unitOfWork.SaveChangesAsync(cancellationToken);

        foreach (SubscriptionDowngradedEvent @event in eventsToPublish)
        {
            await publishEndpoint.Publish(@event, cancellationToken);
        }
    }
}
