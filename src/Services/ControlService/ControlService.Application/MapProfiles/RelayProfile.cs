using AutoMapper;
using BuildingBlocks.IntegrationEvents.Events.Relays;
using Control.Application.Features.Relays.Commands.SyncRelayCreated;
using Control.Application.Features.Relays.Commands.SyncRelayDeleted;
using Control.Application.Features.Relays.Commands.SyncRelayMode;
using Control.Application.Features.Relays.Commands.SyncRelayState;
using Control.Application.Features.Relays.Commands.SyncRelayUpdated;
using Control.Domain.Events;

namespace Control.Application.MapProfiles;

public sealed class RelayProfile : Profile
{
    public RelayProfile()
    {
        CreateMap<RelayCreatedEvent, SyncRelayCreatedCommand>();

        CreateMap<RelayDeletedEvent, SyncRelayDeletedCommand>();

        CreateMap<RelayModeSyncedEvent, SyncRelayModeCommand>();

        CreateMap<RelayStateSyncedEvent, SyncRelayStateCommand>();

        CreateMap<RelayUpdatedEvent, SyncRelayUpdatedCommand>();

        CreateMap<RelayModeChangedDomainEvent, RelayModeChangedEvent>();

        CreateMap<RelayStateChangedDomainEvent, ChangeRelayStateEvent>();
    }
}
