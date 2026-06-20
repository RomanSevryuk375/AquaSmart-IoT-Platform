namespace Device.Application.Features.Telemetry.Command.TransmitTelemetry.TelemtryValidators;

public sealed class TelemetryItemRequestValidator 
    : AbstractValidator<TelemetryItem>
{
    public TelemetryItemRequestValidator()
    {
        RuleLevelCascadeMode = CascadeMode.Stop;

        RuleFor(x => x.SensorId)
            .NotEmpty().WithMessage("SensorId must not be empty.")
            .NotEqual(Guid.Empty).WithMessage("SensorId cannot be an empty Guid.");

        RuleFor(x => x.ExternalMessageId)
            .NotEmpty().WithMessage("ExternalMessageId must not be empty.")
            .MaximumLength(100).WithMessage("ExternalMessageId is too long (max 100).");

        RuleFor(x => x.RecordedAt)
            .NotEmpty().WithMessage("RecordedAt must be provided.")
            .Must(BeInPast).WithMessage("RecordedAt can not be in the future.");
    }

    private bool BeInPast(DateTime recordedAt) =>
        recordedAt < DateTime.UtcNow.AddMinutes(5);
}
