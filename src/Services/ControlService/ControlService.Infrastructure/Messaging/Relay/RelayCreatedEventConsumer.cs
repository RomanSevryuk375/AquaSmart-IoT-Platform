using AutoMapper;
using Contracts.Events.RelayEvents;
using Control.Application.Features.Relays.Commands.SyncRelayCreated;
using MediatR;

namespace Control.Infrastructure.Messaging.Relay;

internal sealed class RelayCreatedEventConsumer(ISender sender, IMapper mapper) :
    MediatRIntegrationEventConsumer<RelayCreatedEvent, SyncRelayCreatedCommand>(sender, mapper);

