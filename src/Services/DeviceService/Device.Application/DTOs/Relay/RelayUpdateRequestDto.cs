using Contracts.Enums;

namespace Device.Application.DTOs.Relay;

public record RelayUpdateRequestDto
{
    public Guid ControllerId { get; init; }
    public ConnectionProtocolEnum ConnectionProtocol { get; init; }
    public string ConnectionAddress { get; init; } = string.Empty;
    public RelayPurposeEnum Purpose { get; init; }
    public bool IsNormalyOpen { get; init; }
}
