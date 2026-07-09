namespace Device.TestShared.Constants;

public static class TestConstants
{
    public static readonly Guid ControllerId = Guid.Parse("11111111-1111-1111-1111-111111111111");
    public static readonly Guid UserId = Guid.Parse("22222222-2222-2222-2222-222222222222");
    public static readonly Guid RelayId = Guid.Parse("33333333-3333-3333-3333-333333333333");
    public static readonly Guid SensorId = Guid.Parse("44444444-4444-4444-4444-444444444444");

    public const string ValidMacAddress = "00:1A:2B:3C:4D:5E";
    public const string ValidRawToken = "my_super_secret_token";
    public const string ValidTokenHash = "$2a$11$Z1Qn110XzT2aP23kH6Mv7.R1oXb41p2wE1.0/kX0Vp9H9GjL8wYmW";
    public const string ValidDeviceName = "Test Device";

    public const string ValidI2cAddress = "0x38";
    public const string ValidDigitalAddress = "D2";

    public const ConnectionProtocol ValidProtocol = ConnectionProtocol.I2C;
}
