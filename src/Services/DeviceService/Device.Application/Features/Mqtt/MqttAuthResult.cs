namespace Device.Application.Features.Mqtt;

public sealed record MqttAuthResult(bool Allow, bool IsSuperuser)
{
    public static MqttAuthResult Deny() => new(false, false);
    public static MqttAuthResult AllowDevice() => new(true, false);
    public static MqttAuthResult AllowSuperuser() => new(true, true);
}

public sealed record MqttAclResult(bool Allow)
{
    public static MqttAclResult Deny() => new(false);
    public static MqttAclResult AllowAction() => new(true);
}
