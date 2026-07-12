using AutoMapper;
using Contracts.Events.RelayEvents;
using Control.Application.Features.Relays.Commands.SyncRelayMode;
using MediatR;

namespace Control.Infrastructure.Messaging.Relay;

internal sealed class RelayModeChangedComandConsumer(ISender sender, IMapper mapper) :
    MediatRIntegrationEventConsumer<RelayModeChangedEvent, SyncRelayModeCommand>(sender, mapper);
