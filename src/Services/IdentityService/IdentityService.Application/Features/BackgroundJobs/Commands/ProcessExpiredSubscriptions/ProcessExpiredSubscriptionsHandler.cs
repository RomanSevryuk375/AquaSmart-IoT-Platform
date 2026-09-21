using BuildingBlocks.Domain.Enums;
using BuildingBlocks.Domain.Results;
using IdentityService.Domain.Entities;
using IdentityService.Domain.Interfaces;
using MediatR;

namespace IdentityService.Application.Features.BackgroundJobs.Commands.ProcessExpiredSubscriptions;

public sealed class ProcessExpiredSubscriptionsHandler(IUserRepository userRepository)
    : IRequestHandler<ProcessExpiredSubscriptionsCommand, Result>
{
    public async Task<Result> Handle(ProcessExpiredSubscriptionsCommand request, CancellationToken cancellationToken)
    {
        IReadOnlyList<User> users = await userRepository.GetWithExpiredSubscriptionAsync(cancellationToken);
        foreach (User user in users)
        {
            user.SetSubscription(Guid.Parse(SubscriptionType.Free), SubscriptionType.FreeDuration);
        }

        return Result.Success();
    }
}
