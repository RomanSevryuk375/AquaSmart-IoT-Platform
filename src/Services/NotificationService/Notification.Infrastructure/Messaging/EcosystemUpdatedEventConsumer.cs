using AutoMapper;
using Contracts.Events.EcosystemEvents;
using Contracts.Results;
using MassTransit;
using MediatR;
using Notification.Application.Features.Ecosystems.Commands.SyncEcosystemDeleted;

namespace Notification.Infrastructure.Messaging;

public sealed class EcosystemUpdatedEventConsumer(ISender sender, IMapper mapper)
    : IConsumer<EcosystemUpdatedEvent>
{
    public async Task Consume(ConsumeContext<EcosystemUpdatedEvent> context)
    {
        SyncEcosystemUpdatedCommand command = mapper.Map<SyncEcosystemUpdatedCommand>(context.Message);

        Result result = await sender.Send(command, context.CancellationToken);
        if (result.IsFailure)
        {
            throw new InvalidOperationException(result.Error.Message);
        }
    }
}
