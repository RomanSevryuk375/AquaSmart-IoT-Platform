using AutoMapper;
using Contracts.Events.SensorEvents;
using Control.Application.Features.Sensors.Commands.HandleSensorNoData;
using MediatR;

namespace Control.Infrastructure.Messaging.Sensor;

internal sealed class SensorNoDataEventConsumer(ISender sender, IMapper mapper) :
    MediatRIntegrationEventConsumer<SensorNoDataEvent, HandleSensorNoDataCommand>(sender, mapper);

