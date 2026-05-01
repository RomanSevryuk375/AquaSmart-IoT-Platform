namespace Contracts.Constants;

public static class SensorConstants
{
    public const int UnitLength = 32;
    public const int ConnectionAddressLength = 32;
    public const int NameLength = 128;
}

public static class ControllerConstants
{
    public const string MacAddressRegex = @"^([0-9A-Fa-f]{2}[:-]){5}([0-9A-Fa-f]{2})$";
    public const int MacAddressLength = 17;
    public const int NameLength = 128;
}

public static class RelayConstants
{
    public const int NameLength = 128;
    public const int ConnectionAddressLength = 32;
}

public static class RuleConstants
{
    public const int NameLength = 128;
}

public static class EcosystemConstants
{
    public const int NameLength = 128;
}

public static class MaintenanceLogConstants
{
    public const int Length = 1024;
}

public static class NotificationConstants
{
    public const int messageLength = 2048;
}

public static class ReminderConstants
{
    public const int NameLength = 128;
}

public static class UserConstants
{
    public const int PhoneNumberLength = 128;
    public const int EmailLength = 64;
}