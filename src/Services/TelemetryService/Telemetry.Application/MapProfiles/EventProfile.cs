using AutoMapper;
using Contracts.Events.SensorEvents;
using Telemetry.Domain.Events;

namespace Telemetry.Application.MapProfiles;

public sealed class EventProfile : Profile
{
    public EventProfile()
    {
        CreateMap<SensorNoDataDomainEvent, SensorNoDataEvent>();
    }
}
