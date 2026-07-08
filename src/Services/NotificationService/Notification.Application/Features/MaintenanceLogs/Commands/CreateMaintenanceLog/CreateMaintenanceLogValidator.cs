// Ignore Spelling: Validator

using FluentValidation;

namespace Notification.Application.Features.MaintenanceLogs.Commands.CreateMaintenanceLog;

public sealed class CreateMaintenanceLogValidator
    : AbstractValidator<CreateMaintenanceLogCommand>
{
    public CreateMaintenanceLogValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty();

        RuleFor(x => x.EcosystemId)
            .NotEmpty();

        RuleFor(x => x.ActionDate)
            .NotEmpty()
            .LessThanOrEqualTo(DateTime.UtcNow.AddMinutes(5));

        RuleFor(x => x.Metrics)
            .NotNull();

        RuleFor(x => x.Notes)
            .MaximumLength(1024);
    }
}
