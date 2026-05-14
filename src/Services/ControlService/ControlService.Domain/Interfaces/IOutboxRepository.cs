using Control.Domain.Entities;

namespace Control.Domain.Interfaces;

public interface IOutboxRepository
{
    Task UpdateAsync(
        OutboxMessage entity,
        CancellationToken cancellationToken);

    Task<IReadOnlyList<OutboxMessage>> GetPendingMessagesAsync(
        int batchSize, 
        CancellationToken cancellationToken);
}