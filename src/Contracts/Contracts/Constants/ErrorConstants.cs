namespace Contracts.Constants;


public static class CommonErrors
{
    public const string IdEmpty = "Id must not be empty.";
    public const string NameEmpty = "Device name must not be empty.";
    public static string InvalidNameLength = $"Name must be under {ControllerConstants.NameLength} characters.";
    public const string AddressEmpty = "Connection address must not be empty.";
    public const string I2cInvalidFormat = "Invalid I2C format. Expected format: '0x00' to '0x7F'.";
    public const string I2cInvalid7bitAddress = "I2C 7-bit address must be between 0x00 and 0x7F.";
    public const string OneWireInvalidFormat = "Invalid 1-Wire address. Must be exactly 16 hex characters.";
    public static string InvalidOneWireLength = $"OneWire address length must be {CommonConstants.OneWireLength} symbols.";
    public const string DigitalInvalidFormat = "Invalid digital pin format. Expected formats like 'PA5', 'GPIO13', 'D2', or '13'.";
    public const string AnalogInvalidFormat = "Invalid analog pin format. Expected formats: 'A0', 'ADC5', 'ADC1_CH3', 'ADC2_IN5', or a raw channel number.";
    public const string InvalidAddressProtocol = "Connection protocol not found.";
}

public static class ControllerErrors
{
    public const string ControllerIdEmpty = "ControllerId must not be empty.";
    public const string MacAddressEmpty = "MAC address cannot be empty.";
    public const string InvalidMacAddressFormat = "Invalid MAC address format.";
    public static string InvalidMacAddressLength = $"MAC address length must be {ControllerConstants.MacAddressLength} symbols.";
}

public static class UserErrors
{
    public const string UserIdEmpty = "UserId must not be empty.";
}

public static class SensorErrors
{
    public const string InvalidType = "Sensor type is not supported.";
}

public static class RelayErrors
{
    public const string InvalidPowerSensorType = "Sensor must have voltage type.";
}
