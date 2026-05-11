namespace Device.Application.DTOs.Telemetry;

public sealed record TelemetryBatchRequest
{
    public string MacAddress { get; init; } = string.Empty;
    public List<TelemetryItemRequest> Items { get; init; } = [];
}
