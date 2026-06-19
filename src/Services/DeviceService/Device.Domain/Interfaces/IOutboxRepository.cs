namespace Device.Domain.Interfaces;

public interface IOutboxRepository
{
    public Task<IReadOnlyList<OutboxMessage>> GetPendingMessagesAsync(
        int batchSize, 
        CancellationToken cancellationToken = default);

    public Task UpdateAsync(
        OutboxMessage entity, 
        CancellationToken cancellationToken = default);
}
