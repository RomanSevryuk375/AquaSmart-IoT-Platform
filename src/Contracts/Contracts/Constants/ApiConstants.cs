namespace Contracts.Constants;

public static class ApiConstants
{
    public const string HealthRoute = "/health";

    public static class Routes
    {
        public const string Controllers = "api/device/v1/controllers";
        public const string Commands = "api/device/v1/commands";
        public const string Relays = "api/device/v1/relays";
        public const string Sensors = "api/device/v1/sensors";
    }

    public static class Headers
    {
        public const string MacAddress = "X-Mac-Address";
        public const string DeviceToken = "X-Device-Token";
    }
}
