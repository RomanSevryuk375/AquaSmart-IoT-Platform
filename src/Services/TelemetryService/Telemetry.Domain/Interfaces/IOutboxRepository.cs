using Telemetry.Domain.Entities;

namespace Telemetry.Domain.Interfaces;

public interface IOutboxRepository
{
    Task<IReadOnlyList<OutboxMessage>> GetPendingMessagesAsync(
        int batchSize, 
        CancellationToken cancellationToken);

    Task UpdateAsync(
        OutboxMessage entity,
        CancellationToken cancellationToken);
}