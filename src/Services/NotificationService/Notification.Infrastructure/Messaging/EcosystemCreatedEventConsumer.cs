using AutoMapper;
using Contracts.Events.EcosystemEvents;
using Contracts.Results;
using MassTransit;
using MediatR;
using Notification.Application.Features.Ecosystems.Commands.SyncEcosystemCreated;

namespace Notification.Infrastructure.Messaging;

public sealed class EcosystemCreatedEventConsumer(ISender sender, IMapper mapper)
    : IConsumer<EcosystemCreatedEvent>
{
    public async Task Consume(ConsumeContext<EcosystemCreatedEvent> context)
    {
        SyncEcosystemCreatedCommand command = mapper.Map<SyncEcosystemCreatedCommand>(context.Message);

        Result result = await sender.Send(command, context.CancellationToken);
        if (result.IsFailure)
        {
            throw new InvalidOperationException(result.Error.Message);
        }
    }
}
