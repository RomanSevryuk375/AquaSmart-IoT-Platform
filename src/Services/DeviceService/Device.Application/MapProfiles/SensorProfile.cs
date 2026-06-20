using Contracts.Events.RelayEvents;
using Contracts.Events.SensorEvents;
using Device.Application.DTOs.Configurations;
using Device.Domain.DomainEvents.SensorEvents;
using Device.Domain.Events.RelayEvents;
using Device.Domain.Events.SensorEvents;

namespace Device.Application.MapProfiles;

public sealed class SensorProfile : Profile
{
    public SensorProfile()
    {
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

        CreateMap<SensorCreatedDomainEvent, SensorCreatedEvent>();

        CreateMap<SensorStateChangedDomainEvent, SensorStateChangedEvent>();

        CreateMap<SensorUpdatedDomainEvent, SensorUpdatedEvent>();

        CreateMap<SetRelayPowerSensorDomainEvent, SetRelayPowerSensorEvent>(); 
    }
}
