namespace Control.Infrastructure.Persistence.Outbox;

public interface IOutboxRepository
{
    public Task<IReadOnlyList<OutboxMessage>> GetPendingMessagesAsync(
        int batchSize,
        CancellationToken cancellationToken);
}
