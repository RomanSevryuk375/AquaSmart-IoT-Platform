using Contracts.Enums;

namespace Device.Application.DTOs.Configurations;

public sealed record RelayConfigDto
{
    public Guid RelayId { get; init; }
    public string Name { get; init; } = string.Empty;
    public ConnectionProtocolEnum ConnectionProtocol { get; init; }
    public string ConnectionAddress { get; init; } = string.Empty;
    public bool IsNormalyOpen { get; init; }
    public RelayPurposeEnum Purpose { get; init; }
    public bool IsActive { get; init; }
    public bool IsManual { get; init; }
}
