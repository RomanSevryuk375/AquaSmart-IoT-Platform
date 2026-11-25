namespace Device.Application.Extesions;

public sealed record MqttOptions
{
    public const string SectionName = "MqttOptions";

    public string Host { get; init; } = "localhost";
    public int Port { get; init; } = 1883;
    public bool UseTls { get; init; }
    public string Username { get; init; } = "device-service-worker";
    public string Password { get; init; } = string.Empty;
    public string ClientIdPrefix { get; init; } = "aqua-device-service";
    public string SharedGroup { get; init; } = "device-service";
}

public sealed record MqttBrokerOptions
{
    public const string SectionName = "MqttBrokerOptions";

    public string WebhookSecret { get; init; } = string.Empty;
}
