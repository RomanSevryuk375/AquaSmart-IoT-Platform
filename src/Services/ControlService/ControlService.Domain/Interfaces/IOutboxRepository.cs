using Control.Domain.Entities;

namespace Control.Domain.Interfaces;

public interface IOutboxRepository
{
    public Task<IReadOnlyList<OutboxMessage>> GetPendingMessagesAsync(
        int batchSize,
        CancellationToken cancellationToken);
}
