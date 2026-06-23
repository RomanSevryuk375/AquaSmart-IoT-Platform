using Contracts.Abstractions;
using Device.Application.Features.Sensors.Query.Shared;

namespace Device.Application.Features.Sensors.Query.GetSensorById;

public sealed record GetSensorByIdQuery
    : IQuery<Result<SensorDto>>
{
    public Guid UserId { get; init; }
    public Guid SensorId { get; init; }
}
