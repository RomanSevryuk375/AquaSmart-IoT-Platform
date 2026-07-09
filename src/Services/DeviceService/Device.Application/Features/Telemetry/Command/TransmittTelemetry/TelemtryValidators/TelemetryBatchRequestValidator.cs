// Ignore Spelling: Validator

namespace Device.Application.Features.Telemetry.Command.TransmittTelemetry.TelemtryValidators;

internal sealed class TelemetryBatchRequestValidator
    : AbstractValidator<TransmitTelemetryCommand>
{
    public TelemetryBatchRequestValidator()
    {
        RuleFor(x => x.MacAddress)
            .NotEmpty().WithMessage(ValidationMessages.MacAddressRequired)
            .Matches(ControllerConstants.MacAddressRegex).WithMessage("Invalid MacAddress format.");

        RuleFor(x => x.Items)
            .NotNull().WithMessage(ValidationMessages.ItemsListNull)
            .NotEmpty().WithMessage(ValidationMessages.TelemetryBatchEmpty)
            .Must(x => x.Count <= 50).WithMessage(ValidationMessages.MaxBatchSize);

        RuleForEach(x => x.Items).SetValidator(new TelemetryItemRequestValidator());
    }
}
