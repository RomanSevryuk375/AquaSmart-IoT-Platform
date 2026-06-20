using Contracts.Abstractions;

namespace Device.Application.Features.Telemetry.Command.TransmitTelemetry;

internal sealed record TransmitTelemetryCommand : ICommand<TelemetryTransmitedResponse>
{
    public string DeviceToken { get; init; } = string.Empty;
    public string MacAddress { get; init; } = string.Empty;
    public List<TelemetryItem> Items { get; init; } = [];
}
