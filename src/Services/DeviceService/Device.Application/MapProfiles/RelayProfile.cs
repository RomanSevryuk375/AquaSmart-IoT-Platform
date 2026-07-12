using Contracts.Events.RelayEvents;
using Device.Application.Features.Controllers.Query.GetControllerConfig;
using Device.Application.Features.RelayCommands.Command.SetRelayState;
using Device.Application.Features.RelayCommands.Query.GetPending;
using Device.Application.Features.Relays.Command.AddRelay;
using Device.Domain.Events.RelayEvents;

namespace Device.Application.MapProfiles;

public sealed class RelayProfile : Profile
{
    public RelayProfile()
    {
        CreateMap<Relay, RelayCreatedResponse>();

        CreateMap<Relay, RelayCreatedEvent>()
            .ForMember(desc => desc.RelayId,
                       opt => opt.MapFrom(src => src.Id));

        CreateMap<Relay, RelayUpdatedEvent>()
            .ForMember(desc => desc.RelayId,
                       opt => opt.MapFrom(src => src.Id));

        CreateMap<Relay, RelayConfig>()
            .ForMember(desc => desc.RelayId,
                       opt => opt.MapFrom(src => src.Id));

        CreateMap<RelayCommand, RelayCommandDto>();

        CreateMap<RelayCreatedDomainEvent, RelayCreatedEvent>();

        CreateMap<RelayModeChangedDomainEvent, RelayModeChangedEvent>();

        CreateMap<RelayStateChangedDomainEvent, ChangeRelayStateEvent>();

        CreateMap<RelayUpdatedDomainEvent, RelayUpdatedEvent>();

        CreateMap<ChangeRelayStateEvent, SetRelayStateCommand>();
    }
}
