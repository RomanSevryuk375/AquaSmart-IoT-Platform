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

public static class SensorConstants
{
    public const int UnitLength = 32;
}

public static class ControllerConstants
{
    public const string MacAddressRegex = @"^([0-9A-Fa-f]{2}[:-]){5}([0-9A-Fa-f]{2})$";
    public const int MacAddressLength = 17;
}

public static class MaintenanceLogConstants
{
    public const int Length = 1024;
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
