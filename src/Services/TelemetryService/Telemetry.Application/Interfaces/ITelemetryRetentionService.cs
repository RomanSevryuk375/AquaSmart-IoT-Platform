namespace Telemetry.Application.Interfaces;

public interface ITelemetryRetentionService
{
    public Task CleanUpOldDataAsync(
        CancellationToken cancellationToken);
}