using AutoMapper;
using Contracts.Events.SensorEvents;
using Control.Application.Features.Sensors.Commands.SyncSensorName;
using MediatR;

namespace Control.Infrastructure.Messaging.Sensor;

internal sealed class SensorRenamedEventConsumer(ISender sender, IMapper mapper) :
    MediatRIntegrationEventConsumer<SensorRenamedEvent, SyncSensorNameCommand>(sender, mapper);
