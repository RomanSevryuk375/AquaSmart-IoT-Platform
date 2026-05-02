using Contracts.Enums;
using Contracts.Events.UserEvents;
using IdentityService.Application.Interfaces;
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
        var users = await userRepository
            .GetWithExpiredSubscriptionAsync(cancellationToken);

        var eventsToPublish = new List<SubscriptionDowngradedEvent>();

        foreach (var user in users)
        {
            user.SetSubscription(Guid
                .Parse(SubscriptionEnum.Free), SubscriptionEnum.FreeDuration);

            await userRepository
                .UpdateAsync(user, cancellationToken);

            eventsToPublish.Add(new SubscriptionDowngradedEvent
            {
                UserId = user.Id,
                NewSubscriptionId = user.SubscriptionId,
                OccurredOn = DateTime.UtcNow,
            });
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);

        foreach(var @event in eventsToPublish)
        {
            await publishEndpoint.Publish(@event, cancellationToken);
        }
    }
}
