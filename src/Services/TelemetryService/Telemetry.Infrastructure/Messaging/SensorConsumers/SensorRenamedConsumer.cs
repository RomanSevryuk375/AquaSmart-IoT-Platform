using AutoMapper;
using Contracts.Events.SensorEvents;
using MediatR;
using Telemetry.Application.Features.Sensors.Commands.SyncSensorNameChanged;

namespace Telemetry.Infrastructure.Messaging.SensorConsumers;

internal sealed class SensorRenamedConsumer(ISender sender, IMapper mapper) :
    MediatRIntegrationEventConsumer<SensorRenamedEvent, SyncSensorNameChangedCommand>(sender, mapper);

