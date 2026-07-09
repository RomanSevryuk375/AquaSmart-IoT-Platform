using AutoMapper;
using Contracts.Events.SensorEvents;
using Telemetry.Application.Features.Sensors.Commands.SyncSensorCreated;
using Telemetry.Application.Features.Sensors.Commands.SyncSensorDeleted;
using Telemetry.Application.Features.Sensors.Commands.SyncSensorNameChanged;
using Telemetry.Application.Features.Sensors.Commands.SyncSensorStateChanged;
using Telemetry.Application.Features.Sensors.Commands.SyncSensorUpdated;

namespace Telemetry.Application.MapProfiles;

public sealed class SensorProfile : Profile
{
    public SensorProfile()
    {
        CreateMap<SensorCreatedEvent, SyncSensorCreatedCommand>();

        CreateMap<SensorDeletedEvent, SyncSensorDeletedCommand>();

        CreateMap<SensorUpdatedEvent, SyncSensorUpdatedCommand>();

        CreateMap<SensorRenamedEvent, SyncSensorNameChangedCommand>();

        CreateMap<SensorStateChangedEvent, SyncSensorStateChangedCommand>();

        CreateMap<SyncSensorUpdatedCommand, SyncSensorCreatedCommand>();
    }
}
