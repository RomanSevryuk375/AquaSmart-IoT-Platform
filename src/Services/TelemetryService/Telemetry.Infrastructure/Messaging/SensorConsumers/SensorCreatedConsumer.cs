using AutoMapper;
using Contracts.Events.SensorEvents;
using MediatR;
using Telemetry.Application.Features.Sensors.Commands.SyncSensorCreated;

namespace Telemetry.Infrastructure.Messaging.SensorConsumers;

internal sealed class SensorCreatedConsumer(ISender sender, IMapper mapper) :
    MediatRIntegrationEventConsumer<SensorCreatedEvent, SyncSensorCreatedCommand>(sender, mapper);
