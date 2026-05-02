namespace Device.Application.DTOs.Configurations;

public sealed record ConfigResponseDto
{
    public int SendIntervalMs { get; init; }
    public int MaxBatchSize { get; init; }
    public IReadOnlyList<RelayConfigDto> Relays { get; init; } = [];
    public IReadOnlyList<SensorConfigDto> Sensors { get; init; } = []; 
}
