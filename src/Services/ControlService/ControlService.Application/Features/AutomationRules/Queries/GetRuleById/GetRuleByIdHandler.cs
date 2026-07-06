using System.Data;
using Contracts.Results;
using Control.Application.DTOs.AutomationRule;
using Control.Domain.Interfaces;
using Control.Domain.ValueObjects;
using Dapper;
using MediatR;

namespace Control.Application.Features.AutomationRules.Queries.GetRuleById;

internal class GetRuleByIdHandler(ISqlConnectionFactory sqlConnectionFactory)
    : IRequestHandler<GetRuleByIdQuery, Result<AutomationRuleDto>>
{
    public async Task<Result<AutomationRuleDto>> Handle(
        GetRuleByIdQuery request,
        CancellationToken cancellationToken)
    {
        using IDbConnection connection = sqlConnectionFactory.CreateConnection();

        const string SQL = """
            SELECT
              r.id,
              r.ecosystem_id,
              r.relay_id,
              r.name,
              r.operator,
              r.action,
              r.is_active,
              r.created_at
            FROM automation_rules r
            INNER JOIN ecosystems e ON r.ecosystem_id = e.id
            WHERE r.id = @RuleId
            AND e.user_id = @UserId;

            SELECT
              rc.id,
              rc.sensor_id,
              rc.condition,
              rc.condition_threshold,
              rc.automation_rule_id,
              rc.created_at
            FROM rule_conditions rc
              INNER JOIN automation_rules r ON rc.automation_rule_id = r.id
              INNER JOIN ecosystems e ON r.ecosystem_id = e.id
            WHERE r.id = @RuleId
            AND e.user_id = @UserId;
            """;

        using SqlMapper.GridReader multi = await connection.QueryMultipleAsync(SQL, new
        {
            request.RuleId,
            request.UserId
        });

        AutomationRuleDto? rule = await multi.ReadSingleOrDefaultAsync<AutomationRuleDto>();
        if (rule is null)
        {
            return Result<AutomationRuleDto>.Failure(Error.NotFound<Rule>(
                $"Rule {request.RuleId} not found"));
        }

        IEnumerable<RuleConditionFlat> conditionsFlat = await multi.ReadAsync<RuleConditionFlat>();
        var conditions = conditionsFlat.Select(c =>
        {
            var parsedThreshold = ConditionThreshold.Parse(c.ConditionThreshold);
            return new RuleConditionResponseDto
            {
                Id = c.Id,
                SensorId = c.SensorId,
                Condition = c.Condition,
                Threshold = parsedThreshold.Threshold,
                Hysteresis = parsedThreshold.Hysteresis
            };
        }).ToList();

        rule = rule with { Conditions = conditions.AsReadOnly() };

        return Result<AutomationRuleDto>.Success(rule);
    }
}

internal sealed record RuleConditionFlat
{
    public Guid Id { get; init; }
    public Guid SensorId { get; init; }
    public Contracts.Enums.Condition Condition { get; init; }
    public string ConditionThreshold { get; init; } = string.Empty;
    public Guid AutomationRuleId { get; init; }
}
