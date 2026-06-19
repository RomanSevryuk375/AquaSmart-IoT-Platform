using Contracts.Events.SensorEvents;
using Device.Application.DTOs.Configurations;

namespace Device.Application.MapProfiles;

public sealed class SensorProfile : Profile
{
    public SensorProfile()
    {
        CreateMap<Sensor, SensorResponseDto>();

        CreateMap<Sensor, SensorUpdatedEvent>()
            .ForMember(desc => desc.SensorId,
                       opt => opt.MapFrom(src => src.Id));

        CreateMap<Sensor, SensorStateChangedEvent>()
            .ForMember(desc => desc.SensorId,
                       opt => opt.MapFrom(src => src.Id));

        CreateMap<Sensor, SensorCreatedEvent>()
            .ForMember(desc => desc.SensorId,
                       opt => opt.MapFrom(src => src.Id));

        CreateMap<Sensor, SensorConfigDto>()
            .ForMember(desc => desc.SensorId,
                       opt => opt.MapFrom(src => src.Id));
    }
}
