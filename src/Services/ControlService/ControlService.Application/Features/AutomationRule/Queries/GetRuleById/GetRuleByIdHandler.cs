using AutoMapper;
using Contracts.Results;
using Control.Domain.Entities;
using Control.Domain.Interfaces;
using MediatR;

namespace Control.Application.Features.AutomationRule.Queries.GetRuleById;

public sealed class GetRuleByIdHandler(
    IAutomationRuleRepository ruleRepository,
    IMapper mapper) : IRequestHandler<GetRuleByIdQuery, Result<AutomationRuleDto>>
{
    public async Task<Result<AutomationRuleDto>> Handle(
        GetRuleByIdQuery request,
        CancellationToken cancellationToken)
    {
        AutomationRuleEntity? rule = await ruleRepository.GetByIdAsync(request.RuleId, cancellationToken);
        if (rule is null)
        {
            return Result<AutomationRuleDto>.Failure(Error.NotFound(
                "Rule.NotFound", $"Rule {request.RuleId} not found"));
        }

        return Result<AutomationRuleDto>.Success(mapper.Map<AutomationRuleDto>(rule));
    }
}
