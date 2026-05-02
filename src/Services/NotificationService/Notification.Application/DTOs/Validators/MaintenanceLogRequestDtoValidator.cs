using Contracts.Constants;
using FluentValidation;
using Notification.Application.DTOs.MaintenanceLog;

namespace Notification.Application.DTOs.Validators;

public sealed class MaintenanceLogRequestDtoValidator
    : AbstractValidator<MaintenanceLogRequestDto>
{
    public MaintenanceLogRequestDtoValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty();

        RuleFor(x => x.EcosystemId)
            .NotEmpty();

        RuleFor(x => x.ActionDate)
            .NotEmpty()
            .LessThan(DateTime.UtcNow.AddMinutes(1));

        RuleFor(x => x.Metrics)
            .NotEmpty();

        RuleFor(x => x.Notes)
            .MaximumLength(MaintenanceLogConstants.Length);
    }
}
