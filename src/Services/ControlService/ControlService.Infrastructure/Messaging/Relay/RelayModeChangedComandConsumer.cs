using AutoMapper;
using BuildingBlocks.IntegrationEvents.Events.Relays;
using Control.Application.Features.Relays.Commands.SyncRelayMode;
using MediatR;

namespace Control.Infrastructure.Messaging.Relay;

internal sealed class RelayModeChangedComandConsumer(ISender sender, IMapper mapper) :
    MediatRIntegrationEventConsumer<RelayModeSyncedEvent, SyncRelayModeCommand>(sender, mapper);
