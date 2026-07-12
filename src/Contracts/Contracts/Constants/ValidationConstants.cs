namespace Contracts.Constants;

public static class CommonConstants
{
    public const int NameLength = 128;
    public const Int32 MinI2cnumericValue = 0x00;
    public const Int32 MaxI2cnumericValue = 0x7F;
    public const string I2cRegex = @"^0[xX][0-9A-Fa-f]{1,2}$";
    public const int OneWireLength = 16;
    public const string OneWireRegex = @"^[0-9A-Fa-f]{16}$";
    public const string DigitalRegex = @"^([A-Za-z]{1,4})?[0-9]{1,2}$";
    public const string AnalogRegex = @"^(A\d{1,2}|ADC\d{1,2}|ADC[1-9]_(CH|IN)\d{1,2}|\d{1,2})$";
    public const int ConnectionAddressLength = 32;
}

public static class IdentityConstants
{
    public const string PhoneNumberRegex = @"^(\+375|80)(29|44|33|25)\d{7}$";
    public const string EmailNumberRegex = @"^[^@\s]+@[^@\s]+\.[^@\s]+$";
    public const int EmailLength = 256;
    public const int PasswordLength = 8;
}

public static class SensorConstants
{
    public const int UnitLength = 32;
    public const int NameLength = CommonConstants.NameLength;
}

public static class ControllerConstants
{
    public const string MacAddressRegex = @"^([0-9A-Fa-f]{2}[:-]){5}([0-9A-Fa-f]{2})$";
    public const int MacAddressLength = 17;
    public const int NameLength = CommonConstants.NameLength;
}

public static class RelayConstants
{
    public const int NameLength = CommonConstants.NameLength;
    public const int ConnectionAddressLength = CommonConstants.ConnectionAddressLength;
}

public static class RelayCommandConstants
{
    public const int MaxAttemptCount = 3;
    public const int RetryCooldownMinutes = 1;
    public const int DefaultExpiryMinutes = 5;
}

public static class RuleConstants
{
    public const int NameLength = CommonConstants.NameLength;
}

public static class EcosystemConstants
{
    public const int NameLength = CommonConstants.NameLength;
}

public static class ReminderConstants
{
    public const int NameLength = CommonConstants.NameLength;
}

public static class MaintenanceLogConstants
{
    public const int Length = 1024;
    public const int MaxFutureDelayMinutes = 5;
}

public static class NotificationConstants
{
    public const int MessageLength = 2048;
}

public static class UserConstants
{
    public const int PhoneNumberLength = 128;
    public const int EmailLength = 64;
}

public static class TelemetryConstants
{
    public const int MaxFutureDelayMinutes = 5;
}
