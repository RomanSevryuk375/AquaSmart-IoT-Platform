using Device.Domain.Entities;
using Device.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Device.Infrastructure.Persistance.Repositories;

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
