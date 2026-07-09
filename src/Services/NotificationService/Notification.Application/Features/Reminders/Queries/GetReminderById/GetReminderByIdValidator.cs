using FluentValidation;

namespace Notification.Application.Features.Reminders.Queries.GetReminderById;

public sealed class GetReminderByIdValidator : AbstractValidator<GetReminderByIdQuery>
{
    public GetReminderByIdValidator()
    {
        RuleFor(x => x.ReminderId).NotEmpty();
        RuleFor(x => x.UserId).NotEmpty();
    }
}
