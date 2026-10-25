namespace Mobile.Services;

/// <summary>
/// Additional API endpoints for newly integrated features (Vacation Modes, Schedules, Reminders, Logs, Firmware OTA, Telegram)
/// Keeping these here keeps ApiConfiguration.cs clean so local network IP overrides are preserved.
/// </summary>
public static class MobileApiEndpoints
{
    // Identity & Telegram Bot
    public const string TelegramLinkToken = "api/identity/v1/profile/telegram-link-token";

    // Vacation Modes (ControlService)
    public const string VacationModes = "api/control/v1/vacation-modes";

    // Relay Schedules (ControlService)
    public const string Schedules = "api/control/v1/schedules";

    // Maintenance Logs (NotificationService)
    public const string MaintenanceLogs = "api/notification/v1/maintenance-logs";

    // Reminders & Tasks (NotificationService)
    public const string Reminders = "api/notification/v1/reminders";

    // Firmware OTA (FirmwareService)
    public const string FirmwareOtaCheck = "api/firmware/v1/ota/check";

    // Real-Time Telemetry Hub (TelemetryService SignalR)
    public const string TelemetryHub = "hubs/raw-telemetry";
}
