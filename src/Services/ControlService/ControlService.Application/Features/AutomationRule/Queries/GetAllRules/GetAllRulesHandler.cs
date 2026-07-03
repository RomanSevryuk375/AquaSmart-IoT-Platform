using AutoMapper;
using Contracts.Results;
using Control.Domain.Entities;
using Control.Domain.Interfaces;
using Control.Domain.SpecificationParams;
using Control.Domain.Specifications;
using MediatR;

namespace Control.Application.Features.AutomationRule.Queries.GetAllRules;

public sealed class GetAllRulesHandler(
    IAutomationRuleRepository ruleRepository,
    IMapper mapper) : IRequestHandler<GetAllRulesQuery, Result<IReadOnlyList<AutomationRuleDto>>>
{
    public async Task<Result<IReadOnlyList<AutomationRuleDto>>> Handle(
        GetAllRulesQuery request,
        CancellationToken cancellationToken)
    {
        var specification = new AutomationRuleFilterSpecification(
            new AutomationRuleFilterParams
            {
                EcosystemId = request.EcosystemId,
                RelayId = request.RelayId,
                Action = request.Action,
                Operator = request.Operator,
            });

        IReadOnlyList<AutomationRuleEntity> rules = await ruleRepository.GetAllAsync(
            specification, cancellationToken);

        return Result<IReadOnlyList<AutomationRuleDto>>.Success(
            mapper.Map<IReadOnlyList<AutomationRuleDto>>(rules));
    }
}
