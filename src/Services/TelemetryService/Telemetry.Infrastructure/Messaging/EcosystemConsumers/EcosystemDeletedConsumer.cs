using AutoMapper;
using Contracts.Events.EcosystemEvents;
using MediatR;
using Telemetry.Application.Features.Ecosystems.Commands.SyncEcosystemDeleted;

namespace Telemetry.Infrastructure.Messaging.EcosystemConsumers;

internal sealed class EcosystemDeletedConsumer(ISender sender, IMapper mapper) :
    MediatRIntegrationEventConsumer<EcosystemDeletedEvent, SyncEcosystemDeletedCommand>(sender, mapper);
