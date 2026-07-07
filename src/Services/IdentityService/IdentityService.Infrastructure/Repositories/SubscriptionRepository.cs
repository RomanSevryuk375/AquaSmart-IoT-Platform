using IdentityService.Domain.Entities;
using IdentityService.Domain.Interfaces;

namespace IdentityService.Infrastructure.Repositories;

public sealed class SubscriptionRepository(IdentityDbContext dbContext)
    : BaseRepository<Subscription>(dbContext), ISubscriptionRepository
{
}
