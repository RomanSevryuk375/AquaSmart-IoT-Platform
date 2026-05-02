namespace Telemetry.Application.Interfaces;

public interface ITelemetryRetentionService
{
    Task CleanUpOldDataAsync(
        CancellationToken cancellationToken);
}