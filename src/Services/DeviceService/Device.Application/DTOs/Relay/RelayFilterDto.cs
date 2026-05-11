using Contracts.Enums;

namespace Device.Application.DTOs.Relay;

public sealed record RelayFilterDto
{
    public Guid? ControllerId { get; init; }
    public RelayPurposeEnum? Purpose { get; init; }
    public bool? IsActive { get; init; }
    public bool? IsManual { get; init; }
}
