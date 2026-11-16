using Microsoft.AspNetCore.SignalR.Client;
using Mobile.Services.Dto.Telemetry;
using Mobile.Services.Interfaces;

namespace Mobile.Services;

public sealed class TelemetryHubService : ITelemetryHubService
{
    private readonly IIdentityService _identityService;
    private HubConnection? _hubConnection;
    private readonly SemaphoreSlim _connectionLock = new(1, 1);
    private readonly HashSet<Guid> _subscribedEcosystems = new();

    public event EventHandler<TelemetryRawChartPointDto>? RawTelemetryReceived;

    public bool IsConnected => _hubConnection?.State == HubConnectionState.Connected;

    public TelemetryHubService(IIdentityService identityService)
    {
        _identityService = identityService;
    }

    public async Task ConnectAsync(CancellationToken cancellationToken = default)
    {
        if (IsConnected) return;

        await _connectionLock.WaitAsync(cancellationToken);
        try
        {
            if (IsConnected) return;

            if (_hubConnection == null)
            {
                var baseUrl = ApiConfiguration.BaseUrl.TrimEnd('/');
                var hubUrl = $"{baseUrl}/{MobileApiEndpoints.TelemetryHub.TrimStart('/')}";

                _hubConnection = new HubConnectionBuilder()
                    .WithUrl(hubUrl, options =>
                    {
                        options.AccessTokenProvider = async () => await _identityService.GetTokenAsync();
                        options.HttpMessageHandlerFactory = handler =>
                        {
                            if (handler is HttpClientHandler clientHandler)
                            {
                                clientHandler.ServerCertificateCustomValidationCallback = (_, _, _, _) => true;
                            }
                            return handler;
                        };
                    })
                    .WithAutomaticReconnect(new[]
                    {
                        TimeSpan.Zero,
                        TimeSpan.FromSeconds(2),
                        TimeSpan.FromSeconds(5),
                        TimeSpan.FromSeconds(10)
                    })
                    .Build();

                _hubConnection.On<TelemetryRawChartPointDto>("TelemetryRawReceived", point =>
                {
                    RawTelemetryReceived?.Invoke(this, point);
                });

                _hubConnection.Reconnected += connectionId =>
                {
                    // Re-subscribe to all active ecosystems after reconnection
                    lock (_subscribedEcosystems)
                    {
                        foreach (var ecoId in _subscribedEcosystems)
                        {
                            _ = SafeSubscribeAsync(ecoId);
                        }
                    }
                    return Task.CompletedTask;
                };
            }

            if (_hubConnection.State == HubConnectionState.Disconnected)
            {
                await _hubConnection.StartAsync(cancellationToken);
            }
        }
        catch
        {
            // Fail gracefully if server is unreachable or offline
        }
        finally
        {
            _connectionLock.Release();
        }
    }

    public async Task DisconnectAsync()
    {
        await _connectionLock.WaitAsync();
        try
        {
            if (_hubConnection != null && _hubConnection.State != HubConnectionState.Disconnected)
            {
                await _hubConnection.StopAsync();
            }
        }
        catch
        {
        }
        finally
        {
            _connectionLock.Release();
        }
    }

    public async Task SubscribeToEcosystemAsync(Guid ecosystemId, CancellationToken cancellationToken = default)
    {
        lock (_subscribedEcosystems)
        {
            _subscribedEcosystems.Add(ecosystemId);
        }

        await ConnectAsync(cancellationToken);
        await SafeSubscribeAsync(ecosystemId);
    }

    private async Task SafeSubscribeAsync(Guid ecosystemId)
    {
        try
        {
            if (_hubConnection?.State == HubConnectionState.Connected)
            {
                await _hubConnection.InvokeAsync("SubscribeToRawTelemetry", ecosystemId.ToString());
            }
        }
        catch
        {
        }
    }

    public async Task UnsubscribeFromEcosystemAsync(Guid ecosystemId, CancellationToken cancellationToken = default)
    {
        lock (_subscribedEcosystems)
        {
            _subscribedEcosystems.Remove(ecosystemId);
        }

        try
        {
            if (_hubConnection?.State == HubConnectionState.Connected)
            {
                await _hubConnection.InvokeAsync("UnsubscribeFromRawTelemetry", ecosystemId.ToString(), cancellationToken);
            }
        }
        catch
        {
        }
    }

    public async ValueTask DisposeAsync()
    {
        if (_hubConnection != null)
        {
            try
            {
                await _hubConnection.DisposeAsync();
            }
            catch
            {
            }
            _hubConnection = null;
        }
        _connectionLock.Dispose();
    }
}
