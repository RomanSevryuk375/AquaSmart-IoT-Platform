using System.Text.RegularExpressions;
using BuildingBlocks.Domain.Constants;
using BuildingBlocks.Domain.Results;

namespace Device.Domain.ValueObjects;

public sealed partial record MacAddress
{
    public string Value { get; } = string.Empty;

    internal MacAddress(string macAddress)
    {
        Value = macAddress;
    }

    public static Result<MacAddress> Create(string macAddress)
    {
        if (string.IsNullOrWhiteSpace(macAddress))
        {
            return Result<MacAddress>.Failure(Error.Validation<MacAddress>(
                ControllerErrors.MacAddressEmpty));
        }

        string cleanMac = macAddress.Trim();

        if (string.IsNullOrWhiteSpace(cleanMac))
        {
            return Result<MacAddress>.Failure(Error.Validation<MacAddress>(
                ControllerErrors.MacAddressEmpty));
        }

        if (cleanMac.Length != ControllerConstants.MacAddressLength)
        {
            return Result<MacAddress>.Failure(Error.Validation<MacAddress>(
                ControllerErrors.InvalidMacAddressLength));
        }

        if (!MacAddressRegex().IsMatch(cleanMac))
        {
            return Result<MacAddress>.Failure(Error.Validation<MacAddress>(
                ControllerErrors.InvalidMacAddressFormat));
        }

        return Result<MacAddress>.Success(
            new MacAddress(cleanMac.ToUpperInvariant()));
    }

    public static MacAddress Parse(string dbVal) => new(dbVal);

    [GeneratedRegex(ControllerConstants.MacAddressRegex, RegexOptions.CultureInvariant)]
    private static partial Regex MacAddressRegex();

    public override string ToString() => Value;

}
