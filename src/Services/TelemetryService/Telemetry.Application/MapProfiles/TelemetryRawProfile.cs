using AutoMapper;
using Contracts.Events.TelemetryEvents;
using Telemetry.Application.DTOs;
using Telemetry.Domain.Entities;

namespace Telemetry.Application.MapProfiles;

public sealed class TelemetryRawProfile : Profile
{
    public TelemetryRawProfile()
    {
        CreateMap<TelemetryRawEntity, TelemetryRawChartPointDto>();

        CreateMap<TelemetryBatchEventItem, TelemetryReceivedEvent>();
    }
}
