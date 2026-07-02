using Contracts.Abstractions;
using Contracts.Results;
using Device.Infrastructure.Persistence.Outbox;
using MediatR;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Device.Infrastructure.BackgroundJobs;

public sealed class OutboxMessageProcessorService(
    IOutboxRepository outboxRepository,
    IPublisher publisher,
    IUnitOfWork unitOfWork,
    ILogger<OutboxMessageProcessorService> logger)
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
            if (type is null)
            {
                logger.LogWarning("Outbox message deserialization failed: Type {MessageType} not found for message {MessageId}", message.Type, message.Id);
                message.Error = $"Type {message.Type} not found.";
                message.ProcessedOnUtc = DateTime.UtcNow;
                continue;
            }

            IDomainEvent? domainEvent;
            try
            {
                domainEvent = JsonConvert.DeserializeObject(message.Content, type) as IDomainEvent;
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Outbox message deserialization failed: Content of message {MessageId} could not be deserialized to {MessageType}", message.Id, message.Type);
                message.Error = ex.Message;
                message.ProcessedOnUtc = DateTime.UtcNow;
                continue;
            }

            if (domainEvent is null)
            {
                logger.LogWarning("Outbox message deserialization failed: Content of message {MessageId} is not an IDomainEvent", message.Id);
                message.Error = "Content is not an IDomainEvent.";
                message.ProcessedOnUtc = DateTime.UtcNow;
                continue;
            }

            try
            {
                await publisher.Publish(domainEvent, cancellationToken);
                message.ProcessedOnUtc = DateTime.UtcNow;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to publish domain event {MessageType} for outbox message {MessageId}", message.Type, message.Id);
                message.Error = ex.Message;
                return Result.Failure(Error.Failure("Outbox.PublishError", ex.Message));
            }
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
