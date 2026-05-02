namespace Device.Application.DTOs.Telemetry;

public record TelemetryResponse
{
    public int AcceptedCount { get; set; }
    public List<string> ValidationErrors { get; set; } = [];
    public int SkippedCount { get; set; }
}
