using Contracts.Abstractions;
using Contracts.Results;
using MediatR;
using Newtonsoft.Json;
using Telemetry.Domain.Interfaces;
using Telemetry.Infrastructure.Persistence.Outbox;

namespace Telemetry.Infrastructure.BackgroundJobs;

public sealed class OutboxMessageProcessorService(
    IOutboxRepository outboxRepository,
    IPublisher publisher,
    IUnitOfWork unitOfWork)
{
    public async Task<Result> ProcessAsync(CancellationToken cancellationToken = default)
    {
        int batchSize = 50;

        IReadOnlyList<OutboxMessage>? messages = await outboxRepository.GetPendingMessagesAsync(
            batchSize, cancellationToken);
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

            if (JsonConvert.DeserializeObject(message.Content, type) is not IDomainEvent domainEvent)
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
