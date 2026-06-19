using Contracts.Abstractions;
using Contracts.Results;
using Device.Infrastructure.Persistence.Outbox;
using MediatR;
using Newtonsoft.Json;

namespace Device.Infrastructure.BackgroundJobs;

public sealed class OutboxMessageProcessorService(
    IOutboxRepository outboxRepository,
    IPublisher publisher,
    IUnitOfWork unitOfWork)
{
    public async Task<Result> ProcessAsync(CancellationToken cancellationToken)
    {
        int batchSize = 50;

        IReadOnlyList<OutboxMessage>? messages = await outboxRepository.GetPendingMessagesAsync(
            batchSize, cancellationToken);
        if (messages is null || !messages.Any())
        {
            return Result.Success();
        }

        foreach (OutboxMessage message in messages)
        {
            var type = Type.GetType(message.Type);
            if (type == null)
            {
                continue;
            }

            if (JsonConvert.DeserializeObject(message.Content, type) is not IDomainEvent domainEvent)
            {
                continue;
            }

            await publisher.Publish(domainEvent, cancellationToken);

            message.ProcessedOnUtc = DateTime.UtcNow;
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
