using Mobile.Services.Dto.Telemetry;

namespace Mobile.Services.Interfaces;

public interface ITelemetryHubService : IAsyncDisposable
{
    bool IsConnected { get; }
    Task ConnectAsync(CancellationToken cancellationToken = default);
    Task DisconnectAsync();
    Task SubscribeToEcosystemAsync(Guid ecosystemId, CancellationToken cancellationToken = default);
    Task UnsubscribeFromEcosystemAsync(Guid ecosystemId, CancellationToken cancellationToken = default);
    event EventHandler<TelemetryRawChartPointDto>? RawTelemetryReceived;
}
