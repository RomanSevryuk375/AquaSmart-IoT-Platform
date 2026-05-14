using Contracts.Abstractions;
using Contracts.Results;
using Control.Application.Interfaces;
using Control.Domain.Interfaces;
using MediatR;
using Newtonsoft.Json;

namespace Control.Application.Services;

public sealed class OutboxMessageProcessorService(
    IOutboxRepository outboxRepository,
    IPublisher publisher,
    IUnitOfWork unitOfWork) : IOutboxMessageProcessorService
{
    public async Task<Result> ProcessAsync(CancellationToken cancellationToken)
    {
        var batchSize = 50;

        var messages = await outboxRepository.GetPendingMessagesAsync(batchSize, cancellationToken);
        if (messages is null ||
            !messages.Any())
        {
            return Result.Success();
        }

        foreach (var message in messages)
        {
            var type = Type.GetType(message.Type);
            if (type == null)
            {
                continue;
            }

            var domainEvent = JsonConvert.DeserializeObject(message.Content, type) as IDomainEvent;

            if (domainEvent is null)
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

