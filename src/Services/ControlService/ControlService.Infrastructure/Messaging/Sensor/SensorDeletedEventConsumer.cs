using AutoMapper;
using Contracts.Events.SensorEvents;
using Control.Application.Features.Sensors.Commands.SyncSensorDeleted;
using MediatR;

namespace Control.Infrastructure.Messaging.Sensor;

internal sealed class SensorDeletedEventConsume(ISender sender, IMapper mapper) :
    MediatRIntegrationEventConsumer<SensorDeletedEvent, SyncSensorDeletedCommand>(sender, mapper);
