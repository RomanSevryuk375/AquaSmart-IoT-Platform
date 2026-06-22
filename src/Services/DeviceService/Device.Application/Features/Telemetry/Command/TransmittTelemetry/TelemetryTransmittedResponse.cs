namespace Device.Application.Features.Telemetry.Command.TransmittTelemetry;

public sealed record TelemetryTransmittedResponse
{
    public int AcceptedCount { get; set; }
    public List<string> ValidationErrors { get; set; } = [];
    public int SkippedCount { get; set; }
}
