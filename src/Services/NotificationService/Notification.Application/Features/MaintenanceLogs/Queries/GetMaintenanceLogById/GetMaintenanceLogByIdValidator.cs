using FluentValidation;

namespace Notification.Application.Features.MaintenanceLogs.Queries.GetMaintenanceLogById;

public sealed class GetMaintenanceLogByIdValidator : AbstractValidator<GetMaintenanceLogByIdQuery>
{
    public GetMaintenanceLogByIdValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.UserId).NotEmpty();
    }
}
