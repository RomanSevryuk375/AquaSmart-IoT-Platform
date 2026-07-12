using AutoMapper;
using Contracts.Events.EcosystemEvents;
using MediatR;
using Notification.Application.Features.Ecosystems.Commands.SyncEcosystemDeleted;

namespace Notification.Infrastructure.Messaging.Ecosystem;

internal sealed class EcosystemDeletedEventConsumer(ISender sender, IMapper mapper) :
    MediatRIntegrationEventConsumer<EcosystemDeletedEvent, SyncEcosystemDeletedCommand>(sender, mapper);
