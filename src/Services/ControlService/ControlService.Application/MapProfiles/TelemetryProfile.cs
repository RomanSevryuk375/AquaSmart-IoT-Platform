using AutoMapper;
using Contracts.Events.TelemetryEvents;
using Control.Application.Features.Telemetry.Commands.ProcessTelemetry;

namespace Control.Application.MapProfiles;

public sealed class TelemetryProfile : Profile
{
    public TelemetryProfile()
    {
        CreateMap<TelemetryReceivedEvent, ProcessTelemetryCommand>();
    }
}
