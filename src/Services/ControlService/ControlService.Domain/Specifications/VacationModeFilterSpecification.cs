using Contracts.Abstractions;
using Control.Domain.Entities;
using Control.Domain.SpecificationParams;

namespace Control.Domain.Specifications;

public sealed class VacationModeFilterSpecification
    : BaseSpecification<VacationMode>
{
    public VacationModeFilterSpecification(VacationModeFilterParams @params)
        : base(data =>
            (!@params.EcosystemId.HasValue || data.EcosystemId == @params.EcosystemId.Value)
            && (!@params.StartDate.HasValue || data.DateRange.StartDate > @params.StartDate.Value)
            && (!@params.EndDate.HasValue || data.DateRange.EndDate < @params.EndDate.Value)
            && (!@params.IsActive.HasValue || data.IsActive == @params.IsActive.Value))
    {

    }
}
