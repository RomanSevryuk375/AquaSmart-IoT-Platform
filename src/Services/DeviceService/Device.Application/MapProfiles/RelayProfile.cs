using Contracts.Events.RelayEvents;
using Device.Application.DTOs.Configurations;

namespace Device.Application.MapProfiles;

public sealed class RelayProfile : Profile
{
    public RelayProfile()
    {
        CreateMap<Relay, RelayResponseDto>()
            .ForMember(desc => desc.ConnectionAddress,
                       opt => opt.MapFrom(src => 
                            $"{src.ConnectionProtocol}.{src.ConnectionAddress}"));

        CreateMap<Relay, RelayCreatedEvent>()
            .ForMember(desc => desc.RelayId,
                       opt => opt.MapFrom(src => src.Id));

        CreateMap<Relay, RelayUpdatedEvent>()
            .ForMember(desc => desc.RelayId,
                       opt => opt.MapFrom(src => src.Id));

        CreateMap<Relay, RelayConfigDto>()
            .ForMember(desc => desc.RelayId,
                       opt => opt.MapFrom(src => src.Id));
    }
}
