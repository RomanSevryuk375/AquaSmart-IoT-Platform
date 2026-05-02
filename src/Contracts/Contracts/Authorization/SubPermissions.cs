namespace Contracts.Authorization;

public static class SubPermissions
{
    // --- Логические сущности (Аквариумы/Террариумы) ---
    public const string TankRead = "tank:read";
    public const string TankCreate = "tank:create";
    public const string TankUpdate = "tank:update";
    public const string TankDelete = "tank:delete";

    // Лимиты (для проверки в ControlService)
    public const string TankLimit1 = "tank:limit:1";   
    public const string TankLimit10 = "tank:limit:10";
    public const string TankLimitUnlimited = "tank:limit:unlim";

    // --- Управление оборудованием (Device Service) ---
    public const string DeviceControl = "device:control";     // Вкл/выкл реле
    public const string DeviceEditManual = "device:manual";   // Право переключать Manual Mode

    // --- Автоматизация (Control Service) ---
    public const string AutoRuleCreate = "auto:rule:create";
    public const string AutoRuleLimit5 = "auto:rule:limit:5"; // Лимит на кол-во правил
    public const string AutoRuleLimit10 = "auto:rule:limit:10"; // Лимит на кол-во правил
    public const string AutoRuleUnlimited = "auto:rule:limit:unlim";

    public const string AutoScheduleCreate = "auto:schedule:create";
    public const string VacationMode = "auto:vacation";

    // --- Данные и Аналитика (Telemetry Service) ---
    public const string TelemetryView = "data:view";
    public const string AnalyticsHistory = "data:history";    // Просмотр истории за большой период
    public const string DiagnosticsFull = "data:diag";        // Доступ к мониторингу тока и детекции поломок
    public const string DataRealtime = "data:rt";             // Доступ к SignalR (живые графики)

    // --- Обслуживание и Уведомления (Notification Service) ---
    public const string MaintenanceLogRead = "notify:log:read";
    public const string MaintenanceLogWrite = "notify:log:write";
    public const string ReminderManage = "notify:reminder";   // Создание напоминаний

    public const string EmailAlerts = "notify:email";
    public const string TelegramAlerts = "notify:tg";

    // --- Управление аккаунтом и Биллинг (Identity Service) ---
    public const string AccountView = "account:view";
    public const string AccountUpdate = "account:update";
}