namespace Device.Application.Features.Controllers.Query.GetControllerConfig;

public sealed record ControllerConfig
{
    public int SendIntervalMs { get; init; }
    public int MaxBatchSize { get; init; }
    public IReadOnlyList<RelayConfig> Relays { get; init; } = [];
    public IReadOnlyList<SensorConfig> Sensors { get; init; } = [];
}
