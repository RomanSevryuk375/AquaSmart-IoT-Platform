using AutoMapper;
using Contracts.Events.SensorEvents;
using MassTransit;
using MediatR;
using Telemetry.Domain.Events;

namespace Telemetry.Application.Handlers;

public sealed class SensorNoDataHandler(IPublishEndpoint publishEndpoint, IMapper mapper)
    : INotificationHandler<SensorNoDataDomainEvent>
{
    public async Task Handle(SensorNoDataDomainEvent notification, CancellationToken cancellationToken) =>
        await publishEndpoint.Publish(mapper.Map<SensorNoDataEvent>(notification), cancellationToken);
}
