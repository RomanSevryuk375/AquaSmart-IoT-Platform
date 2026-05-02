using AutoMapper;
using Contracts.Events.SensorEvents;
using Device.Application.DTOs.Configurations;
using Device.Application.DTOs.Sensor;
using Device.Domain.Entities;

namespace Device.Application.MapProfiles;

public sealed class SensorProfile : Profile
{
    public SensorProfile()
    {
        CreateMap<SensorEntity, SensorResponseDto>();

        CreateMap<SensorEntity, SensorUpdatedEvent>()
            .ForMember(desc => desc.SensorId,
                       opt => opt.MapFrom(src => src.Id));

        CreateMap<SensorEntity, SensorStateChangedCommand>()
            .ForMember(desc => desc.SensorId,
                       opt => opt.MapFrom(src => src.Id));

        CreateMap<SensorEntity, SensorCreatedEvent>()
            .ForMember(desc => desc.SensorId,
                       opt => opt.MapFrom(src => src.Id));

        CreateMap<SensorEntity, SensorConfigDto>()
            .ForMember(desc => desc.SensorId,
                       opt => opt.MapFrom(src => src.Id));
    }
}
