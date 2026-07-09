using AutoMapper;
using Telemetry.Application.DTOs;
using Telemetry.Domain.Entities;

namespace Telemetry.Application.MapProfiles;

public sealed class TelemetryAggregateProfile : Profile
{
    public TelemetryAggregateProfile()
    {
        CreateMap<AggregateTelemetry, TelemetryChartPointDto>()
            .ForMember(desc => desc.Time,
                       opt => opt.MapFrom(src => src.PeriodStart));
    }
}
