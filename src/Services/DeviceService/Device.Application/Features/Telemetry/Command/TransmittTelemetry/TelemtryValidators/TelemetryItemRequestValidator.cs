// Ignore Spelling: Validator

namespace Device.Application.Features.Telemetry.Command.TransmittTelemetry.TelemtryValidators;

public sealed class TelemetryItemRequestValidator
    : AbstractValidator<TelemetryItem>
{
    private const int MaxExternalMessageLength = 100;

    public TelemetryItemRequestValidator()
    {
        RuleLevelCascadeMode = CascadeMode.Stop;

        RuleFor(x => x.SensorId)
            .NotEmpty().WithMessage(ValidationMessages.SensorIdEmpty)
            .NotEqual(Guid.Empty).WithMessage(ValidationMessages.SensorIdEmptyGuid);

        RuleFor(x => x.ExternalMessageId)
            .NotEmpty().WithMessage(ValidationMessages.ExternalMessageIdEmpty)
            .MaximumLength(MaxExternalMessageLength).WithMessage(ValidationMessages.ExternalMessageIdTooLong);

        RuleFor(x => x.RecordedAt)
            .NotEmpty().WithMessage(ValidationMessages.RecordedAtProvided)
            .Must(BeInPast).WithMessage(ValidationMessages.RecordedAtFuture);
    }

    private bool BeInPast(DateTime recordedAt) => recordedAt < DateTime.UtcNow.AddMinutes(5);
}
