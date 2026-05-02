using AutoMapper;
using Contracts.Events.RelayEvents;
using Device.Application.DTOs.Configurations;
using Device.Application.DTOs.Relay;
using Device.Domain.Entities;

namespace Device.Application.MapProfiles;

public sealed class RelayProfile : Profile
{
    public RelayProfile()
    {
        CreateMap<RelayEntity, RelayResponseDto>()
            .ForMember(desc => desc.ConnectionAddress,
                       opt => opt.MapFrom(src => 
                            $"{src.ConnectionProtocol}.{src.ConnectionAddress}"));

        CreateMap<RelayEntity, RelayCreatedEvent>()
            .ForMember(desc => desc.RelayId,
                       opt => opt.MapFrom(src => src.Id));

        CreateMap<RelayEntity, RelayUpdatedEvent>()
            .ForMember(desc => desc.RelayId,
                       opt => opt.MapFrom(src => src.Id));

        CreateMap<RelayEntity, RelayConfigDto>()
            .ForMember(desc => desc.RelayId,
                       opt => opt.MapFrom(src => src.Id));
    }
}
