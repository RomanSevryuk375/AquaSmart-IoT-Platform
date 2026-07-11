using Control.Application.DTOs.AutomationRule;
using Control.Application.Features.AutomationRules.Commands.CreateRule;
using Control.Infrastructure.Persistence.Outbox;

namespace Control.Infrastructure.IntegrationTests.Features.AutomationRules;

public class CreateRuleHandlerTests(
    IntegrationTestWebAppFactory factory) : BaseIntegrationTest(factory)
{
    [Fact]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "<Pending>")]
    public async Task Handle_ShouldCreateRuleAndInsertOutboxMessage()
    {
        // Arrange
        Result<Ecosystem> ecosystemResult = Ecosystem.Create(
            Guid.NewGuid(),
            UserContext.UserId,
            EcosystemType.Aquarium,
            "Test Ecosystem",
            100.0,
            Guid.NewGuid());
        Ecosystem ecosystem = ecosystemResult.Value;

        Relay relay = new RelayBuilder()
            .WithId(Guid.NewGuid())
            .WithEcosystemId(ecosystem.Id)
            .Build();

        Sensor sensor = new SensorBuilder()
            .WithId(Guid.NewGuid())
            .WithEcosystemId(ecosystem.Id)
            .Build();

        DbContext.Set<Ecosystem>().Add(ecosystem);
        DbContext.Set<Relay>().Add(relay);
        DbContext.Set<Sensor>().Add(sensor);
        await DbContext.SaveChangesAsync();

        var command = new CreateRuleCommand
        {
            EcosystemId = ecosystem.Id,
            RelayId = relay.Id,
            Name = "Temperature Control",
            Operator = Operator.AND,
            Action = RuleAction.SwitchOn,
            IsActive = true,
            Conditions =
            [
                new RuleConditionRequestDto
                {
                    SensorId = sensor.Id,
                    Condition = Condition.Greater,
                    Threshold = 25.0,
                    Hysteresis = 1.0
                }
            ]
        };

        // Act
        Result<Guid> result = await Sender.Send(command);

        // Assert
        result.IsSuccess.Should().BeTrue(result.Error?.Message ?? "No error message");

        Guid ruleId = result.Value;

        AutomationRule? rule = await DbContext.Set<AutomationRule>()
            .Include(r => r.Conditions)
            .AsNoTracking()
            .FirstOrDefaultAsync(r => r.Id == ruleId);

        rule.Should().NotBeNull();
        rule!.Name.Value.Should().Be("Temperature Control");
        rule.Conditions.Should().ContainSingle();
        rule.Conditions.First().SensorId.Should().Be(sensor.Id);

        RuleCondition? dbCondition = await DbContext.Set<RuleCondition>()
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.AutomationRuleId == ruleId);
        dbCondition.Should().NotBeNull();
        dbCondition!.SensorId.Should().Be(sensor.Id);

        List<OutboxMessage> outboxMessages = await DbContext.Set<OutboxMessage>()
            .AsNoTracking()
            .ToListAsync();

        outboxMessages.Should().NotBeEmpty();
    }
}
