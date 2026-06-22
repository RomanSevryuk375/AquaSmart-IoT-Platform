using Contracts.Abstractions;

namespace Device.Application.Features.Telemetry.Command.TransmittTelemetry;

internal sealed record TransmittTelemetryCommand : ICommand<TelemetryTransmittedResponse>
{
    public string DeviceToken { get; init; } = string.Empty;
    public string MacAddress { get; init; } = string.Empty;
    public List<TelemetryItem> Items { get; init; } = [];

}
