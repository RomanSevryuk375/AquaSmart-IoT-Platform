using Contracts.Constants;
using System.Text.RegularExpressions;

namespace Device.Domain.ValueObjects;

public sealed partial record ConnectionAddress
{
    private const int I2cHexPartLength = 2;

    public ConnectionProtocol Protocol { get; }
    public string Address { get; } = string.Empty;

    private ConnectionAddress(ConnectionProtocol protocol, string address)
    {
        Protocol = protocol;
        Address = address;
    }

    public static Result<ConnectionAddress> Create(ConnectionProtocol protocol, string address)
    {
        if (string.IsNullOrWhiteSpace(address))
        {
            return Result<ConnectionAddress>.Failure(Error.Validation<ConnectionAddress>(
                    CommonErrors.AddressEmpty));
        }

        string cleanAddress = address.Trim();

        return protocol switch
        {
            ConnectionProtocol.OneWire => CreateOneWire(cleanAddress),
            ConnectionProtocol.Digital => CreateDigital(cleanAddress),
            ConnectionProtocol.Analog => CreateAnalog(cleanAddress),
            ConnectionProtocol.I2C => CreateI2c(cleanAddress),

            _ => Result<ConnectionAddress>.Failure(Error.Validation<ConnectionAddress>(
                CommonErrors.InvalidAddressProtocol))
        };
    }

    private static Result<ConnectionAddress> CreateI2c(string address)
    {
        if (!I2cRegex().IsMatch(address))
        {
            return Result<ConnectionAddress>.Failure(Error.Validation<ConnectionAddress>(
                    CommonErrors.I2cInvalidFormat));
        }

        string hexPart = address[I2cHexPartLength..];
        int numericValue = Convert.ToInt32(hexPart, 16);

        if (numericValue < CommonConstants.MinI2cnumericValue || 
            numericValue > CommonConstants.MaxI2cnumericValue)
        {
            return Result<ConnectionAddress>.Failure(Error.Validation<ConnectionAddress>(
                CommonErrors.I2cInvalid7bitAddress));
        }

        return Result<ConnectionAddress>.Success(new ConnectionAddress(
            ConnectionProtocol.I2C,
            address));
    }

    private static Result<ConnectionAddress> CreateOneWire(string address)
    {
        if (!OneWireRegex().IsMatch(address))
        {
            return Result<ConnectionAddress>.Failure(Error.Validation<ConnectionAddress>(
                    CommonErrors.OneWireInvalidFormat));
        }

        if (address.Length != CommonConstants.OneWireLength)
        {
            return Result<ConnectionAddress>.Failure(Error.Validation<ConnectionAddress>(
                CommonErrors.InvalidOneWireLength));
        }

        return Result<ConnectionAddress>.Success(new ConnectionAddress(
            ConnectionProtocol.OneWire, 
            address));
    }

    private static Result<ConnectionAddress> CreateDigital(string address)
    {
        if (!DigitalRegex().IsMatch(address))
        {
            return Result<ConnectionAddress>.Failure(Error.Validation<ConnectionAddress>(
                    CommonErrors.DigitalInvalidFormat));
        }

        return Result<ConnectionAddress>.Success(new ConnectionAddress(
            ConnectionProtocol.Digital,
            address));
    }

    private static Result<ConnectionAddress> CreateAnalog(string address)
    {
        if (!AnalogRegex().IsMatch(address))
        {
            return Result<ConnectionAddress>.Failure(Error.Validation<ConnectionAddress>(
                    CommonErrors.AnalogInvalidFormat));
        }

        return Result<ConnectionAddress>.Success(new ConnectionAddress(
            ConnectionProtocol.Analog,
            address));
    }

    [GeneratedRegex(CommonConstants.I2cRegex, RegexOptions.CultureInvariant)]
    private static partial Regex I2cRegex();

    [GeneratedRegex(CommonConstants.OneWireRegex, RegexOptions.CultureInvariant)]
    private static partial Regex OneWireRegex();

    [GeneratedRegex(CommonConstants.DigitalRegex, RegexOptions.CultureInvariant)]
    private static partial Regex DigitalRegex();

    [GeneratedRegex(CommonConstants.AnalogRegex, RegexOptions.CultureInvariant)]
    private static partial Regex AnalogRegex();

    public override string ToString()
    {
        return $"{Protocol}_{Address}";
    }
}
