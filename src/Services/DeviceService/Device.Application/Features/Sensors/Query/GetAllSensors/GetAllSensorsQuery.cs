using Contracts.Abstractions;
using Device.Application.Features.Sensors.Query.Shared;

namespace Device.Application.Features.Sensors.Query.GetAllSensors;

public sealed record GetAllSensorsQuery
    : IQuery<Result<IReadOnlyList<SensorDto>>>
{
    public Guid UserId { get; init; }
    public Guid? ControllerId { get; init; }
    public SensorType? Type { get; init; }
    public SensorState? State { get; init; }
    public int Skip { get; init; } = 0;
    public int Take { get; init; } = 10;
}
