namespace Mobile.Services;

public static class ApiConfiguration
{
    private const string ApiBaseUrlPrefKey = "api_gateway_base_url";

    /// <summary>
    /// Gets or sets the base URL for the API Gateway.
    /// Defaults to http://10.0.2.2:5055/ on Android emulator and http://localhost:5055/ on Windows.
    /// Can be customized by the user for physical device network testing.
    /// </summary>
    public static string BaseUrl
    {
        get
        {
            var saved = Preferences.Default.Get<string>(ApiBaseUrlPrefKey, string.Empty);
            if (!string.IsNullOrWhiteSpace(saved))
            {
                return EnsureTrailingSlash(saved);
            }

            if (DeviceInfo.Platform == DevicePlatform.Android)
            {
                if (DeviceInfo.DeviceType == DeviceType.Physical)
                {
                    // Physical phone on local Wi-Fi network: connects to host PC IP
                    return "http://172.20.10.8:5055/";
                }

                // Android emulator loops back to host machine via 10.0.2.2
                return "http://10.0.2.2:5055/";
            }

            return "http://localhost:5055/";
        }
        set
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                Preferences.Default.Remove(ApiBaseUrlPrefKey);
            }
            else
            {
                Preferences.Default.Set(ApiBaseUrlPrefKey, EnsureTrailingSlash(value));
            }
        }
    }

    private static string EnsureTrailingSlash(string url)
    {
        return url.EndsWith('/') ? url : url + "/";
    }

    public static class Routes
    {
        public const string AuthLogin = "api/identity/v1/auth/login";
        public const string AuthRegister = "api/identity/v1/auth/register";
        public const string AuthRefresh = "api/identity/v1/auth/refresh";
        public const string AuthLogout = "api/identity/v1/auth/logout";
        public const string Profile = "api/identity/v1/profile";

        public const string Ecosystems = "api/control/v1/ecosystems";
        public const string AutomationRules = "api/control/v1/automation-rules";

        public const string Controllers = "api/device/v1/controllers";
        public const string Sensors = "api/device/v1/sensors";
        public const string Relays = "api/device/v1/relays";
        public const string Commands = "api/device/v1/commands";

        public const string TelemetryData = "api/telemetry/v1/data";
    }
}
