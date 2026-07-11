using Contracts.Abstractions;

namespace Device.Application.Features.Integrations.ValidateHardwareAssignment;

public sealed record ValidateHardwareAssignmentQuery
    : IQuery<Result<ValidateHardwareAssignmentDto>>
{
    public Guid SensorId { get; init; }
    public Guid RelayId { get; init; }
}
