using AutoMapper;
using Contracts.Events.SensorEvents;
using Control.Application.Features.Sensors.Commands.HandleSensorNoData;
using Control.Application.Features.Sensors.Commands.SyncSensorCreated;
using Control.Application.Features.Sensors.Commands.SyncSensorDeleted;
using Control.Application.Features.Sensors.Commands.SyncSensorName;
using Control.Application.Features.Sensors.Commands.SyncSensorState;
using Control.Application.Features.Sensors.Commands.SyncSensorUpdated;

namespace Control.Application.MapProfiles;

public sealed class SensorProfile : Profile
{
    public SensorProfile()
    {
        CreateMap<SensorCreatedEvent, SyncSensorCreatedCommand>();

        CreateMap<SensorDeletedEvent, SyncSensorDeletedCommand>();

        CreateMap<SensorNoDataEvent, HandleSensorNoDataCommand>();

        CreateMap<SensorRenamedEvent, SyncSensorNameCommand>();

        CreateMap<SensorStateChangedEvent, SyncSensorStateCommand>();

        CreateMap<SensorUpdatedEvent, SyncSensorUpdatedCommand>();

        CreateMap<SyncSensorUpdatedCommand, SyncSensorCreatedCommand>();
    }
}
