using Contracts.Abstractions;

namespace Control.Application.Features.Telemetry.Commands.ProcessTelemetry;

public sealed record ProcessTelemetryCommand : ICommand
{
    public Guid SensorId { get; init; }
    public double Value { get; init; }
    public DateTime RecordedAt { get; init; }
}
