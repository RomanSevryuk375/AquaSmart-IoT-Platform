using Contracts.Abstractions;
using Notification.Domain.Entities;
using Notification.Domain.SpecificationParams;

namespace Notification.Domain.Specifications;

public class ReminderFilterSpecification 
    : BaseSpecification<ReminderEntity>
{
    public ReminderFilterSpecification(
        ReminderSpecificationParams @params)
        : base(data =>
            (!@params.UserId.HasValue || data.UserId == @params.UserId.Value) &&
            (!@params.EcosystemId.HasValue || data.EcosystemId == @params.EcosystemId.Value) &&
            (string.IsNullOrWhiteSpace(@params.SearchTerm) ||
             data.TaskName.ToLower().Contains(@params.SearchTerm.ToLower())) &&
            (!@params.LastDoneAtFrom.HasValue || (data.LastDoneAt.HasValue 
                && data.LastDoneAt >= @params.LastDoneAtFrom.Value)) &&
            (!@params.LastDoneAtTo.HasValue || (data.LastDoneAt.HasValue 
                && data.LastDoneAt <= @params.LastDoneAtTo.Value)) &&
            (!@params.NextDueAtFrom.HasValue || data.NextDueAt >= @params.NextDueAtFrom.Value) &&
            (!@params.NextDueAtTo.HasValue || data.NextDueAt <= @params.NextDueAtTo.Value))
    {
    }
}
