using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using BuildingBlocks.Domain.Results;
using Device.Application.Extesions;
using Device.Application.Features.Mqtt;
using Device.Application.Interfaces;
using Device.Domain.ValueObjects;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Formatter;
using MQTTnet.Protocol;

namespace Device.Infrastructure.Mqtt;

public sealed class MqttGatewayBackgroundService : BackgroundService, IMqttCommandPublisher
{
    private const string DevicesTopicPrefix = "aquasmart/devices/";
    private static readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.Never,
    };

    private readonly IServiceScopeFactory _serviceScopeFactory;
    private readonly MqttOptions _options;
    private readonly DeviceSettings _deviceSettings;
    private readonly ILogger<MqttGatewayBackgroundService> _logger;
    private readonly IMqttClient _mqttClient;
    private readonly SemaphoreSlim _connectLock = new(1, 1);
    private CancellationToken _stoppingToken = CancellationToken.None;

    public MqttGatewayBackgroundService(
        IServiceScopeFactory serviceScopeFactory,
        IOptions<MqttOptions> options,
        IOptions<DeviceSettings> deviceSettings,
        ILogger<MqttGatewayBackgroundService> logger)
    {
        _serviceScopeFactory = serviceScopeFactory;
        _options = options.Value;
        _deviceSettings = deviceSettings.Value;
        _logger = logger;
        _mqttClient = new MqttFactory().CreateMqttClient();

        _mqttClient.ConnectedAsync += HandleConnectedAsync;
        _mqttClient.DisconnectedAsync += HandleDisconnectedAsync;
        _mqttClient.ApplicationMessageReceivedAsync += HandleMessageReceivedAsync;
    }

    public async Task<bool> PublishRelayCommandAsync(
        string macAddress,
        RelayCommandMqttPayload payload,
        CancellationToken cancellationToken = default)
    {
        return await PublishJsonAsync(macAddress, "commands", payload, cancellationToken);
    }

    public async Task<bool> PublishConfigNotifyAsync(
        string macAddress,
        ConfigNotifyMqttPayload payload,
        CancellationToken cancellationToken = default)
    {
        return await PublishJsonAsync(macAddress, "config/notify", payload, cancellationToken);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _stoppingToken = stoppingToken;

        if (!_options.Enabled)
        {
            _logger.LogInformation("MQTT gateway worker is disabled via MqttOptions:Enabled=false.");
            return;
        }

        MqttClientOptions clientOptions = BuildClientOptions();

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                if (!_mqttClient.IsConnected)
                {
                    await _connectLock.WaitAsync(stoppingToken);
                    try
                    {
                        if (!_mqttClient.IsConnected)
                        {
                            _logger.LogInformation(
                                "Connecting MQTT gateway worker to {Host}:{Port} (UseTls={UseTls})...",
                                _options.Host,
                                _options.Port,
                                _options.UseTls);

                            await _mqttClient.ConnectAsync(clientOptions, stoppingToken);
                        }
                    }
                    finally
                    {
                        _connectLock.Release();
                    }
                }

                await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(
                    ex,
                    "MQTT gateway worker connection failed for {Host}:{Port}. Retrying in 3 seconds...",
                    _options.Host,
                    _options.Port);

                try
                {
                    await Task.Delay(TimeSpan.FromSeconds(3), stoppingToken);
                }
                catch (OperationCanceledException)
                {
                    break;
                }
            }
        }

        try
        {
            if (_mqttClient.IsConnected)
            {
                await _mqttClient.DisconnectAsync(cancellationToken: CancellationToken.None);
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error while disconnecting MQTT gateway worker.");
        }
    }

    public override void Dispose()
    {
        _mqttClient.Dispose();
        _connectLock.Dispose();
        base.Dispose();
    }

    private MqttClientOptions BuildClientOptions()
    {
        string clientId = $"{_options.ClientIdPrefix}-{Guid.NewGuid():N}";
        MqttClientOptionsBuilder builder = new MqttClientOptionsBuilder()
            .WithClientId(clientId)
            .WithTcpServer(_options.Host, _options.Port)
            .WithProtocolVersion(MqttProtocolVersion.V500)
            .WithCleanSession(true)
            .WithKeepAlivePeriod(TimeSpan.FromSeconds(30));

        if (!string.IsNullOrWhiteSpace(_options.Username))
        {
            builder = builder.WithCredentials(_options.Username, _options.Password);
        }

        if (_options.UseTls)
        {
            builder = builder.WithTlsOptions(tls => tls.UseTls());
        }

        return builder.Build();
    }

    private async Task HandleConnectedAsync(MqttClientConnectedEventArgs args)
    {
        string sharedPrefix = $"$share/{_options.SharedGroup}/{DevicesTopicPrefix}+";

        MqttClientSubscribeOptions subscribeOptions = new MqttFactory().CreateSubscribeOptionsBuilder()
            .WithTopicFilter(f => f.WithTopic($"{sharedPrefix}/telemetry").WithQualityOfServiceLevel(MqttQualityOfServiceLevel.AtLeastOnce))
            .WithTopicFilter(f => f.WithTopic($"{sharedPrefix}/commands/ack").WithQualityOfServiceLevel(MqttQualityOfServiceLevel.AtLeastOnce))
            .WithTopicFilter(f => f.WithTopic($"{sharedPrefix}/relays/state").WithQualityOfServiceLevel(MqttQualityOfServiceLevel.AtLeastOnce))
            .WithTopicFilter(f => f.WithTopic($"{sharedPrefix}/status").WithQualityOfServiceLevel(MqttQualityOfServiceLevel.AtLeastOnce))
            .Build();

        await _mqttClient.SubscribeAsync(subscribeOptions, CancellationToken.None);

        _logger.LogInformation(
            "MQTT gateway worker connected and subscribed to shared group '{SharedGroup}'.",
            _options.SharedGroup);
    }

    private Task HandleDisconnectedAsync(MqttClientDisconnectedEventArgs args)
    {
        if (!args.ClientWasConnected)
        {
            return Task.CompletedTask;
        }

        _logger.LogWarning(
            args.Exception,
            "MQTT gateway worker disconnected from broker (Reason: {Reason}). Reconnect loop active.",
            args.Reason);

        return Task.CompletedTask;
    }

    private async Task HandleMessageReceivedAsync(MqttApplicationMessageReceivedEventArgs args)
    {
        string topic = args.ApplicationMessage.Topic ?? string.Empty;
        if (!TryParseDeviceTopic(topic, out string topicMac, out string suffix))
        {
            _logger.LogWarning("MQTT message ignored: unexpected topic format '{Topic}'.", topic);
            return;
        }

        string rawPayload = args.ApplicationMessage.PayloadSegment.Count > 0
            ? Encoding.UTF8.GetString(args.ApplicationMessage.PayloadSegment)
            : string.Empty;

        try
        {
            using IServiceScope scope = _serviceScopeFactory.CreateScope();
            ISender sender = scope.ServiceProvider.GetRequiredService<ISender>();

            switch (suffix)
            {
                case "telemetry":
                    await DispatchTelemetryAsync(sender, topicMac, rawPayload);
                    break;
                case "commands/ack":
                    await DispatchCommandAckAsync(sender, topicMac, rawPayload);
                    break;
                case "status":
                    await DispatchStatusAsync(sender, topicMac, rawPayload);
                    break;
                case "relays/state":
                    await DispatchRelayStateSyncAsync(sender, topicMac, rawPayload);
                    break;
                default:
                    _logger.LogWarning("MQTT message ignored: unsupported suffix '{Suffix}' on topic '{Topic}'.", suffix, topic);
                    break;
            }
        }
        catch (JsonException jsonEx)
        {
            _logger.LogWarning(
                jsonEx,
                "MQTT message dropped due to malformed JSON on topic '{Topic}' (MAC: {Mac}).",
                topic,
                topicMac);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Unhandled error processing MQTT message on topic '{Topic}' (MAC: {Mac}).",
                topic,
                topicMac);
        }
    }

    private async Task DispatchTelemetryAsync(ISender sender, string topicMac, string rawPayload)
    {
        TelemetryMessagePayload? payload = JsonSerializer.Deserialize<TelemetryMessagePayload>(rawPayload, _jsonOptions);
        if (payload is null)
        {
            _logger.LogWarning("MQTT telemetry dropped: null JSON payload for MAC {Mac}.", topicMac);
            return;
        }

        var command = new ProcessMqttTelemetryCommand
        {
            TopicMac = topicMac,
            PayloadMacAddress = payload.MacAddress,
            Items = payload.Items ?? [],
        };

        await sender.Send(command, CancellationToken.None);
    }

    private async Task DispatchCommandAckAsync(ISender sender, string topicMac, string rawPayload)
    {
        CommandAckMessagePayload? payload = JsonSerializer.Deserialize<CommandAckMessagePayload>(rawPayload, _jsonOptions);
        if (payload is null)
        {
            _logger.LogWarning("MQTT command ack dropped: null JSON payload for MAC {Mac}.", topicMac);
            return;
        }

        var command = new ProcessCommandAckCommand
        {
            TopicMac = topicMac,
            CommandId = payload.CommandId,
            Status = payload.Status ?? string.Empty,
            ErrorMessage = payload.ErrorMessage,
            ExecutedAt = payload.ExecutedAt,
        };

        await sender.Send(command, CancellationToken.None);
    }

    private async Task DispatchStatusAsync(ISender sender, string topicMac, string rawPayload)
    {
        DeviceStatusMessagePayload? payload = JsonSerializer.Deserialize<DeviceStatusMessagePayload>(rawPayload, _jsonOptions);
        if (payload is null)
        {
            _logger.LogWarning("MQTT status dropped: null JSON payload for MAC {Mac}.", topicMac);
            return;
        }

        var command = new ProcessDeviceStatusCommand
        {
            TopicMac = topicMac,
            Status = payload.Status ?? string.Empty,
            FirmwareVersion = payload.FwVersion,
            IpAddress = payload.Ip,
            Rssi = payload.Rssi,
            UptimeSec = payload.UptimeSec,
            Timestamp = payload.Timestamp,
        };

        await sender.Send(command, CancellationToken.None);

        if (string.Equals(command.Status.Trim(), "online", StringComparison.OrdinalIgnoreCase))
        {
            SchedulePendingRedelivery(topicMac);
        }
    }

    private async Task DispatchRelayStateSyncAsync(ISender sender, string topicMac, string rawPayload)
    {
        RelayStateSyncMessagePayload? payload = JsonSerializer.Deserialize<RelayStateSyncMessagePayload>(rawPayload, _jsonOptions);
        if (payload is null)
        {
            _logger.LogWarning("MQTT relays/state dropped: null JSON payload for MAC {Mac}.", topicMac);
            return;
        }

        var command = new ProcessRelayStateSyncCommand
        {
            TopicMac = topicMac,
            PayloadMacAddress = payload.MacAddress,
            Source = payload.Source ?? string.Empty,
            RecordedAt = payload.RecordedAt,
            Relays = payload.Relays ?? [],
        };

        await sender.Send(command, CancellationToken.None);
    }

    private void SchedulePendingRedelivery(string topicMac)
    {
        int delayMs = Math.Max(0, _deviceSettings.OnlineRedeliveryDelayMs);

        _ = Task.Run(async () =>
        {
            try
            {
                if (delayMs > 0)
                {
                    await Task.Delay(delayMs, _stoppingToken);
                }

                using IServiceScope scope = _serviceScopeFactory.CreateScope();
                IRelayCommandRetryService retryService = scope.ServiceProvider.GetRequiredService<IRelayCommandRetryService>();
                await retryService.RedeliverPendingForControllerAsync(topicMac, _stoppingToken);
            }
            catch (OperationCanceledException) when (_stoppingToken.IsCancellationRequested)
            {
                // Host is shutting down; ignore cancellation.
            }
            catch (Exception ex)
            {
                _logger.LogWarning(
                    ex,
                    "Delayed pending command redelivery failed for reconnected MAC {Mac}.",
                    topicMac);
            }
        }, _stoppingToken);
    }

    private async Task<bool> PublishJsonAsync<TPayload>(
        string macAddress,
        string topicSuffix,
        TPayload payload,
        CancellationToken cancellationToken)
    {
        if (!_options.Enabled || !_mqttClient.IsConnected)
        {
            return false;
        }

        Result<MacAddress> macResult = MacAddress.Create(macAddress);
        if (macResult.IsFailure)
        {
            _logger.LogWarning(
                "Cannot publish MQTT {Suffix}: invalid MAC address '{MacAddress}'.",
                topicSuffix,
                macAddress);
            return false;
        }

        string compactMac = macResult.Value.ToCompactHex();
        string topic = $"{DevicesTopicPrefix}{compactMac}/{topicSuffix}";
        string json = JsonSerializer.Serialize(payload, _jsonOptions);

        MqttApplicationMessage message = new MqttApplicationMessageBuilder()
            .WithTopic(topic)
            .WithPayload(json)
            .WithQualityOfServiceLevel(MqttQualityOfServiceLevel.AtLeastOnce)
            .WithRetainFlag(false)
            .Build();

        try
        {
            MqttClientPublishResult result = await _mqttClient.PublishAsync(message, cancellationToken);
            if (!result.IsSuccess)
            {
                _logger.LogWarning(
                    "MQTT publish to '{Topic}' returned non-success reason code {ReasonCode}.",
                    topic,
                    result.ReasonCode);
            }

            return result.IsSuccess;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "MQTT publish failed for topic '{Topic}'.", topic);
            return false;
        }
    }

    private static bool TryParseDeviceTopic(string topic, out string mac, out string suffix)
    {
        mac = string.Empty;
        suffix = string.Empty;

        if (!topic.StartsWith(DevicesTopicPrefix, StringComparison.Ordinal))
        {
            return false;
        }

        string remainder = topic[DevicesTopicPrefix.Length..];
        int slashIndex = remainder.IndexOf('/');
        if (slashIndex <= 0 || slashIndex >= remainder.Length - 1)
        {
            return false;
        }

        mac = remainder[..slashIndex];
        suffix = remainder[(slashIndex + 1)..];
        return true;
    }

    private sealed record TelemetryMessagePayload(
        string? MacAddress,
        List<MqttTelemetryItemDto>? Items);

    private sealed record CommandAckMessagePayload(
        Guid CommandId,
        string? Status,
        string? ErrorMessage,
        DateTime? ExecutedAt);

    private sealed record DeviceStatusMessagePayload(
        string? Status,
        string? FwVersion,
        string? Ip,
        int? Rssi,
        long? UptimeSec,
        DateTime? Timestamp);

    private sealed record RelayStateSyncMessagePayload(
        string? MacAddress,
        string? Source,
        DateTime? RecordedAt,
        List<MqttRelayStateItemDto>? Relays);
}
