using Contracts.Abstractions;
using Contracts.Results;
using Control.Domain.Interfaces;
using Control.Infrastructure.Persistence.Outbox;
using MediatR;
using Newtonsoft.Json;

namespace Control.Infrastructure.BackgroundJobs;

public sealed class OutboxMessageProcessorService(
    IOutboxRepository outboxRepository,
    IPublisher publisher,
    IUnitOfWork unitOfWork)
{
    public async Task<Result> ProcessAsync(CancellationToken cancellationToken)
    {
        int batchSize = 50;

        IReadOnlyList<OutboxMessage>? messages = await outboxRepository.GetPendingMessagesAsync(batchSize, cancellationToken);
        bool anyMessagesForPublish = messages is null || !messages.Any();
        if (anyMessagesForPublish)
        {
            return Result.Success();
        }

        foreach (OutboxMessage message in messages!)
        {
            await PublishDomainEventAsync(message, cancellationToken);
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }

    private async Task PublishDomainEventAsync(
        OutboxMessage message,
        CancellationToken cancellationToken)
    {
        try
        {
            var type = Type.GetType(message.Type);
            if (type == null)
            {
                MarkAsPoisonMessage(message, $"Type '{message.Type}' could not be resolved.");
                return;
            }

            var domainEvent = JsonConvert.DeserializeObject(message.Content, type) as IDomainEvent;
            if (domainEvent is null)
            {
                MarkAsPoisonMessage(message, "Deserialization returned null or invalid type.");
                return;
            }

            await publisher.Publish(domainEvent, cancellationToken);

            message.ProcessedOnUtc = DateTime.UtcNow;
        }
        catch (Exception ex)
        {
            MarkAsPoisonMessage(message, ex.Message);
        }
    }

    private static void MarkAsPoisonMessage(OutboxMessage message, string error)
    {
        message.Error = error;
        message.ProcessedOnUtc = DateTime.UtcNow;
    }
}

