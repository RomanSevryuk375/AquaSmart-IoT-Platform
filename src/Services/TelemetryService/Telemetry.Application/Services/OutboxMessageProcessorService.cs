using Contracts.Abstractions;
using Contracts.Results;
using MediatR;
using Newtonsoft.Json;
using Telemetry.Application.Interfaces;
using Telemetry.Domain.Interfaces;

namespace Telemetry.Application.Services
{
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
                var domainEvent = JsonConvert.DeserializeObject<IDomainEvent>(
                    message.Content,
                    new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.All });

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
}