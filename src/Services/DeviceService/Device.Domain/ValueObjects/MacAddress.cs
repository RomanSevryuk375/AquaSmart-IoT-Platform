using Contracts.Constants;
using System.Text.RegularExpressions;

namespace Device.Domain.ValueObjects;

public sealed partial record MacAddress
{
    public string Value { get; } = string.Empty;

    private MacAddress(string macAddress)
    {
        Value = macAddress;
    }

    public static Result<MacAddress> Create(string macAddress)
    {
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

    [GeneratedRegex(ControllerConstants.MacAddressRegex, RegexOptions.CultureInvariant)]
    private static partial Regex MacAddressRegex();

    public override string ToString() => Value;

}
