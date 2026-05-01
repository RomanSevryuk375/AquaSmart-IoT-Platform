using Contracts.Constants;
using FluentValidation;
using Notification.Application.DTOs.Reminder;

namespace Notification.Application.DTOs.Validators;

public sealed class ReminderRequestDtoValidator
    : AbstractValidator<ReminderRequestDto>
{
    public ReminderRequestDtoValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty();

        RuleFor(x => x.EcosystemId)
            .NotEmpty();

        RuleFor(x => x.TaskName)
            .NotEmpty()
            .MaximumLength(ReminderConstants.NameLength);

        RuleFor(x => x.IntervalDays)
            .NotEmpty()
            .GreaterThan(0);
    }
}
