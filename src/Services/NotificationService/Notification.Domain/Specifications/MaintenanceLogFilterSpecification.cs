using Contracts.Abstractions;
using Notification.Domain.Entities;
using Notification.Domain.SpecificationParams;

namespace Notification.Domain.Specifications;

public class MaintenanceLogFilterSpecification 
    : BaseSpecification<MaintenanceLogEntity>
{
    public MaintenanceLogFilterSpecification(
        MaintenanceLogSpecificationParams @params) 
        : base(data => 
            (!@params.UserId.HasValue || data.UserId == @params.UserId.Value) 
            && (!@params.EcosystemId.HasValue || data.EcosystemId == @params.EcosystemId.Value)
            && (!@params.ActionDateFrom.HasValue || data.ActionDate >= @params.ActionDateFrom.Value)
            && (!@params.ActionDateTo.HasValue || data.ActionDate <= @params.ActionDateTo.Value))
    {
    }
}
