using Device.Domain.Entities;
using Device.Domain.Interfaces;
using Device.Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Device.Infrastructure.Persistence.Outbox;

public sealed class OutboxRepository(DeviceDbContext dbContext)
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
