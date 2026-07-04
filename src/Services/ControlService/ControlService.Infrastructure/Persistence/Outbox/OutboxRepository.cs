using Control.Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Control.Infrastructure.Persistence.Outbox;

public sealed class OutboxRepository(ControlDbContext dbContext)
    : BaseRepository<OutboxMessage>(dbContext), IOutboxRepository
{
    public async Task<IReadOnlyList<OutboxMessage>> GetPendingMessagesAsync(
        int batchSize,
        CancellationToken cancellationToken)
    {
        return await Context.OutboxMessages
            .Where(x => x.ProcessedOnUtc == null)
            .Take(batchSize)
            .ToListAsync(cancellationToken);
    }
}
