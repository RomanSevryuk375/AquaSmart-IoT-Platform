using Microsoft.EntityFrameworkCore;
using Telemetry.Domain.Entities;
using Telemetry.Domain.Interfaces;
using Telemetry.Infrastructure.Persistence.Repositories;

namespace Telemetry.Infrastructure.Persistence.Outbox;

public sealed class OutboxRepository(SystemDbContext dbContext)
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
