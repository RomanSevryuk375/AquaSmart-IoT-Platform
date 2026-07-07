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
        public const string AutomationRules = "api/control/v1/automation-rules";
        public const string Ecosystems = "api/control/v1/ecosystems";
        public const string Schedules = "api/control/v1/schedules";
        public const string VacationModes = "api/control/v1/vacation-modes";
        public const string Profiles = "api/identity/v1/profile";
        public const string Auth = "api/identity/v1/auth";
    }

    public static class Headers
    {
        public const string MacAddress = "X-Mac-Address";
        public const string DeviceToken = "X-Device-Token";
    }
}
