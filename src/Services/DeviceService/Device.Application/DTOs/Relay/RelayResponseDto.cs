using Contracts.Enums;

namespace Device.Application.DTOs.Relay;

public record RelayResponseDto
{
    public Guid Id { get; init; }
    public Guid ControllerId { get; init; }
    public Guid? PowerSensorId { get; init; }
    public string Name { get; init; } = string.Empty;
    public string ConnectionAddress { get; init; } = string.Empty;
    public bool IsNormalyOpen { get; init; }
    public RelayPurposeEnum Purpose { get; init; }
    public bool IsActive { get; init; }
    public bool IsManual { get; init; }
    public DateTime CreatedAt { get; init; }
}
