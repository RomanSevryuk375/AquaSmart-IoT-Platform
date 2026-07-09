using AutoMapper;
using Contracts.Events.EcosystemEvents;
using Contracts.Results;
using MassTransit;
using MediatR;
using Telemetry.Application.Features.Ecosystems.Commands.SyncEcosystemCreated;

namespace Telemetry.Infrastructure.Messaging.EcosystemConsumers;

public sealed class EcosystemCreatedConsumer(ISender sender, IMapper mapper)
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
