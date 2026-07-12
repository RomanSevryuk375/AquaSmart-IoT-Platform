using AutoMapper;
using Contracts.Events.SensorEvents;
using Control.Application.Features.Sensors.Commands.SyncSensorUpdated;
using MediatR;

namespace Control.Infrastructure.Messaging.Sensor;

internal sealed class SensorUpdatedEventConsumer(ISender sender, IMapper mapper) :
    MediatRIntegrationEventConsumer<SensorUpdatedEvent, SyncSensorUpdatedCommand>(sender, mapper);
