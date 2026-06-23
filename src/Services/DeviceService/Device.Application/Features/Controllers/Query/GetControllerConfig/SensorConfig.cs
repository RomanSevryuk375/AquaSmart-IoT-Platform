namespace Device.Application.Features.Controllers.Query.GetControllerConfig;

public sealed record SensorConfig
{
    public Guid SensorId { get; init; }
    public string Name { get; init; } = string.Empty;
    public ConnectionProtocol ConnectionProtocol { get; init; }
    public string ConnectionAddress { get; init; } = string.Empty;
    public SensorType Type { get; init; }
    public string Unit { get; init; } = string.Empty;
}
