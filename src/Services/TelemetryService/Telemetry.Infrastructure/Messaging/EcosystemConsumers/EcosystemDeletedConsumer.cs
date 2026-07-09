using AutoMapper;
using Contracts.Events.EcosystemEvents;
using Contracts.Results;
using MassTransit;
using MediatR;
using Telemetry.Application.Features.Ecosystems.Commands.SyncEcosystemDeleted;

namespace Telemetry.Infrastructure.Messaging.EcosystemConsumers;

public sealed class EcosystemDeletedConsumer(ISender sender, IMapper mapper)
    : IConsumer<EcosystemDeletedEvent>
{
    public async Task Consume(ConsumeContext<EcosystemDeletedEvent> context)
    {
        SyncEcosystemDeletedCommand command = mapper.Map<SyncEcosystemDeletedCommand>(context.Message);

        Result result = await sender.Send(command, context.CancellationToken);
        if (result.IsFailure)
        {
            throw new InvalidOperationException(result.Error.Message);
        }
    }
}
