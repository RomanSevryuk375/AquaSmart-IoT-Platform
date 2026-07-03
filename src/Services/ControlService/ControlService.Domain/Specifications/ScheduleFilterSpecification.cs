using Contracts.Abstractions;
using Control.Domain.Entities;
using Control.Domain.SpecificationParams;

namespace Control.Domain.Specifications;

public sealed class ScheduleFilterSpecification
    : BaseSpecification<ScheduleEntity>
{
    public ScheduleFilterSpecification(ScheduleFilterParams @params)
        : base(data =>
            (!@params.EcosystemId.HasValue || data.EcosystemId == @params.EcosystemId.Value)
            && (!@params.RelayId.HasValue || data.RelayId == @params.RelayId.Value)
            && (!@params.IsFadeMode.HasValue || data.IsFadeMode == @params.IsFadeMode.Value)
            && (!@params.IsEnable.HasValue || data.IsEnable == @params.IsEnable.Value))
    {

    }
}
