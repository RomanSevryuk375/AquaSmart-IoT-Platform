using Contracts.Events.RelayEvents;
using Device.Application.DTOs.Configurations;
using Device.Application.DTOs.RelayCommands;
using Device.Domain.Events.RelayEvents;

namespace Device.Application.MapProfiles;

public sealed class RelayProfile : Profile
{
    public RelayProfile()
    {
        CreateMap<Relay, RelayCreatedEvent>()
            .ForMember(desc => desc.RelayId,
                       opt => opt.MapFrom(src => src.Id));

        CreateMap<Relay, RelayUpdatedEvent>()
            .ForMember(desc => desc.RelayId,
                       opt => opt.MapFrom(src => src.Id));

        CreateMap<Relay, RelayConfigDto>()
            .ForMember(desc => desc.RelayId,
                       opt => opt.MapFrom(src => src.Id));

        CreateMap<RelayCommand, RelayCommandResponseDto>();

        CreateMap<RelayCreatedDomainEvent, RelayCreatedEvent>();

        CreateMap<RelayModeChangedDomainEvent, RelayModeChangedEvent>();

        CreateMap<RelayStateChangedDomainEvent, ChangeRelayStateEvent>();

        CreateMap<RelayUpdatedDomainEvent, RelayUpdatedEvent>();
    }
}
