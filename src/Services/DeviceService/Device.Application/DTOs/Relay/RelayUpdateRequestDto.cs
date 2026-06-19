namespace Device.Application.DTOs.Relay;

public sealed record RelayUpdateRequestDto
{
    public Guid ControllerId { get; init; }
    public ConnectionProtocol ConnectionProtocol { get; init; }
    public string ConnectionAddress { get; init; } = string.Empty;
    public RelayPurposeEnum Purpose { get; init; }
    public bool IsNormalyOpen { get; init; }
}
