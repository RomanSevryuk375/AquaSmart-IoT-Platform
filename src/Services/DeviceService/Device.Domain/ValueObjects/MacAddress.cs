using System.Text.RegularExpressions;
using BuildingBlocks.Domain.Constants;
using BuildingBlocks.Domain.Results;

namespace Device.Domain.ValueObjects;

public sealed partial record MacAddress
{
    public const int CompactMacAddressLength = 12;

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

        if (cleanMac.Length == ControllerConstants.MacAddressLength)
        {
            if (!MacAddressRegex().IsMatch(cleanMac))
            {
                return Result<MacAddress>.Failure(Error.Validation<MacAddress>(
                    ControllerErrors.InvalidMacAddressFormat));
            }

            string canonical = cleanMac.Replace('-', ':').ToUpperInvariant();
            return Result<MacAddress>.Success(new MacAddress(canonical));
        }

        if (cleanMac.Length == CompactMacAddressLength)
        {
            if (!CompactMacAddressRegex().IsMatch(cleanMac))
            {
                return Result<MacAddress>.Failure(Error.Validation<MacAddress>(
                    ControllerErrors.InvalidMacAddressFormat));
            }

            string upper = cleanMac.ToUpperInvariant();
            string canonical = string.Join(':',
                upper[0..2], upper[2..4], upper[4..6],
                upper[6..8], upper[8..10], upper[10..12]);

            return Result<MacAddress>.Success(new MacAddress(canonical));
        }

        return Result<MacAddress>.Failure(Error.Validation<MacAddress>(
            ControllerErrors.InvalidMacAddressLength));
    }

    public static Result<string> NormalizeToCompactHex(string macAddress)
    {
        Result<MacAddress> result = Create(macAddress);
        return result.IsFailure
            ? Result<string>.Failure(result.Error)
            : Result<string>.Success(result.Value.ToCompactHex());
    }

    public string ToCompactHex() => Value.Replace(":", string.Empty, StringComparison.Ordinal);

    public static MacAddress Parse(string dbVal) => new(dbVal);

    [GeneratedRegex(ControllerConstants.MacAddressRegex, RegexOptions.CultureInvariant)]
    private static partial Regex MacAddressRegex();

    [GeneratedRegex(@"^[0-9A-Fa-f]{12}$", RegexOptions.CultureInvariant)]
    private static partial Regex CompactMacAddressRegex();

    public override string ToString() => Value;
}
