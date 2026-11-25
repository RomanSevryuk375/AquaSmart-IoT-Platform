namespace Device.Application.Interfaces;

public sealed record RelayCommandMqttPayload
{
    public Guid CommandId { get; init; }
    public Guid RelayId { get; init; }
    public bool TargetState { get; init; }
    public DateTime? ExpireAt { get; init; }
    public DateTime CreatedAt { get; init; }
}

public sealed record ConfigNotifyMqttPayload
{
    public string Reason { get; init; } = "relay_or_sensor_updated";
    public long Version { get; init; }
    public DateTime Timestamp { get; init; }
}

public interface IMqttCommandPublisher
{
    public Task<bool> PublishRelayCommandAsync(
        string macAddress,
        RelayCommandMqttPayload payload,
        CancellationToken cancellationToken = default);

    public Task<bool> PublishConfigNotifyAsync(
        string macAddress,
        ConfigNotifyMqttPayload payload,
        CancellationToken cancellationToken = default);
}
