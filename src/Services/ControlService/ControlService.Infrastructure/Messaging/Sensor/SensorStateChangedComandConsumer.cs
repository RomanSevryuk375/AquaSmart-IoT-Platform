using AutoMapper;
using Contracts.Events.SensorEvents;
using Control.Application.Features.Sensors.Commands.SyncSensorState;
using MediatR;

namespace Control.Infrastructure.Messaging.Sensor;

internal sealed class SensorStateChangedComandConsumer(ISender sender, IMapper mapper) :
    MediatRIntegrationEventConsumer<SensorStateChangedEvent, SyncSensorStateCommand>(sender, mapper);

