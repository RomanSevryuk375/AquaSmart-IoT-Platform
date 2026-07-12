using AutoMapper;
using Contracts.Events.RelayEvents;
using Control.Application.Features.Relays.Commands.SyncRelayUpdated;
using MediatR;

namespace Control.Infrastructure.Messaging.Relay;

internal sealed class RelayUpdatedEventConsumer(ISender sender, IMapper mapper) :
    MediatRIntegrationEventConsumer<RelayUpdatedEvent, SyncRelayUpdatedCommand>(sender, mapper);
