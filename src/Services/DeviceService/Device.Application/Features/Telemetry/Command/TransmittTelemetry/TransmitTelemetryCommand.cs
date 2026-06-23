using Contracts.Abstractions;

namespace Device.Application.Features.Telemetry.Command.TransmittTelemetry;

public sealed record TransmitTelemetryCommand
    : ICommand<TelemetryTransmittedResponse>
{
    public string DeviceToken { get; init; } = string.Empty;
    public string MacAddress { get; init; } = string.Empty;
    public List<TelemetryItem> Items { get; init; } = [];
}
