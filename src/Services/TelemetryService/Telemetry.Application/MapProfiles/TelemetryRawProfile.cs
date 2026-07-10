using AutoMapper;
using Contracts.Events.TelemetryEvents;
using Telemetry.Application.DTOs;
using Telemetry.Domain.Entities;
using Telemetry.Domain.Events;

namespace Telemetry.Application.MapProfiles;

public sealed class TelemetryRawProfile : Profile
{
    public TelemetryRawProfile()
    {
        CreateMap<RawTelemetry, TelemetryRawChartPointDto>();

        CreateMap<TelemetryBatchEventItem, TelemetryReceivedEvent>();

        CreateMap<TelemetryBatchEventItem, TelemetryRawChartPointDto>();

        CreateMap<RawTelemetryAddedDomainEvent, TelemetryRawChartPointDto>();

        CreateMap<RawTelemetryAddedDomainEvent, TelemetryReceivedEvent>();
    }
}
