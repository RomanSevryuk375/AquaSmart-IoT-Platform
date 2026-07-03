using Contracts.Abstractions;
using Control.Domain.Entities;
using Control.Domain.SpecificationParams;

namespace Control.Domain.Specifications;

public sealed class AutomationRuleFilterSpecification
    : BaseSpecification<AutomationRuleEntity>
{
    public AutomationRuleFilterSpecification(AutomationRuleFilterParams @params)
        : base(data =>
            (!@params.EcosystemId.HasValue || data.EcosystemId == @params.EcosystemId.Value)
            && (!@params.RelayId.HasValue || data.RelayId == @params.RelayId.Value)
            && (!@params.Operator.HasValue || data.Operator == @params.Operator.Value)
            && (!@params.Action.HasValue || data.Action == @params.Action.Value))
    {

    }
}
