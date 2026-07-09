using AutoMapper;
using Contracts.Events.EcosystemEvents;
using Contracts.Results;
using MassTransit;
using MediatR;
using Notification.Application.Features.Ecosystems.Commands.SyncEcosystemDeleted;

namespace Notification.Infrastructure.Messaging;

public sealed class EcosystemDeletedEventConsumer(ISender sender, IMapper mapper)
    : IConsumer<EcosystemDeletedEvent>
{
    public async Task Consume(ConsumeContext<EcosystemDeletedEvent> context)
    {
        SyncEcosystemUpdatedCommand command = mapper.Map<SyncEcosystemUpdatedCommand>(context.Message);

        Result result = await sender.Send(command, context.CancellationToken);
        if (result.IsFailure)
        {
            throw new InvalidOperationException(result.Error.Message);
        }
    }
}
