namespace Telemetry.Infrastructure.Persistence.Outbox;

public interface IOutboxRepository
{
    public Task<IReadOnlyList<OutboxMessage>> GetPendingMessagesAsync(
        int batchSize,
        CancellationToken cancellationToken);

    public Task UpdateAsync(
        OutboxMessage entity,
        CancellationToken cancellationToken);
}
