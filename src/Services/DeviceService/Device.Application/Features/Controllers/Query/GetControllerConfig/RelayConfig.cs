namespace Device.Application.Features.Controllers.Query.GetControllerConfig;

public sealed record RelayConfig
{
    public Guid RelayId { get; init; }
    public string Name { get; init; } = string.Empty;
    public ConnectionProtocol ConnectionProtocol { get; init; }
    public string ConnectionAddress { get; init; } = string.Empty;
    public bool IsNormallyOpen { get; init; }
    public RelayPurpose Purpose { get; init; }
    public bool IsActive { get; init; }
    public bool IsManual { get; init; }
}
