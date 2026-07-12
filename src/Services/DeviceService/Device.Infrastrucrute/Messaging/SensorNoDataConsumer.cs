using AutoMapper;
using Contracts.Events.SensorEvents;
using Device.Application.Features.Sensors.Command.SetSensorState;
using MediatR;

namespace Device.Infrastructure.Messaging;

internal sealed class SensorNoDataConsumer(ISender sender, IMapper mapper) :
    MediatRIntegrationEventConsumer<SensorNoDataEvent, SetSensorStateCommand>(sender, mapper);
