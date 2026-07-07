using IdentityService.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;

namespace IdentityService.Infrastructure.Persistence.Outbox;

public sealed class OutboxRepository(IdentityDbContext dbContext)
    : BaseRepository<OutboxMessage>(dbContext), IOutboxRepository
{
    public async Task<IReadOnlyList<OutboxMessage>> GetPendingMessagesAsync(
        int batchSize,
        CancellationToken cancellationToken = default)
    {
        return await Context.OutboxMessages
            .Where(x => x.ProcessedOnUtc == null)
            .Take(batchSize)
            .ToListAsync(cancellationToken);
    }
}
