namespace Device.Application.Constants;

public static class CacheKeys
{
    public static string Sensor(Guid userId, Guid sensorId) => $"sensor:{userId}:{sensorId}";
    public static string Controller(Guid userId, Guid controllerId) => $"controller:{userId}:{controllerId}";
    public static string ControllerMqttAuth(string canonicalMac) => $"controller:mqtt-auth:{canonicalMac}";
    public static string ControllerTokenAuth(Guid controllerId) => $"controller:token-auth:{controllerId}";
    public static string Relay(Guid userId, Guid relayId) => $"relay:{userId}:{relayId}";
}
