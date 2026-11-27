using System.Text.RegularExpressions;
using BuildingBlocks.Domain.Constants;
using FluentValidation;

namespace Telemetry.Application.Features.Telemetry.Commands.AddTelemetryBatch;

public sealed partial class AddTelemetryBatchValidator : AbstractValidator<AddTelemetryBatchCommand>
{
    public const int MaxBatchSize = 50;
    public const int MaxExternalMessageIdLength = 100;

    public AddTelemetryBatchValidator()
    {
        RuleFor(x => x.MacAddress)
            .NotEmpty()
            .Must(IsValidMacAddress)
            .WithMessage("MacAddress must be a valid MAC address.");

        RuleFor(x => x.Items)
            .Cascade(CascadeMode.Stop)
            .NotEmpty()
            .Must(items => items.Count <= MaxBatchSize)
            .WithMessage($"Batch must contain between 1 and {MaxBatchSize} telemetry items.");

        RuleForEach(x => x.Items).ChildRules(item =>
        {
            item.RuleFor(i => i.SensorId)
                .NotEmpty();

            item.RuleFor(i => i.ExternalMessageId)
                .NotEmpty()
                .MaximumLength(MaxExternalMessageIdLength);
        });
    }

    private static bool IsValidMacAddress(string? macAddress)
    {
        if (string.IsNullOrWhiteSpace(macAddress))
        {
            return false;
        }

        string trimmed = macAddress.Trim();
        return ColonOrDashMacRegex().IsMatch(trimmed) || CompactMacRegex().IsMatch(trimmed);
    }

    [GeneratedRegex(ControllerConstants.MacAddressRegex, RegexOptions.CultureInvariant)]
    private static partial Regex ColonOrDashMacRegex();

    [GeneratedRegex(@"^[0-9A-Fa-f]{12}$", RegexOptions.CultureInvariant)]
    private static partial Regex CompactMacRegex();
}
