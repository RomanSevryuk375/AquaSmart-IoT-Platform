using Control.Application.Features.AutomationRules.Commands.AddCondition;

namespace Control.Infrastructure.IntegrationTests.Features.AutomationRules;

public class AddConditionHandlerTests(
    IntegrationTestWebAppFactory factory) : BaseIntegrationTest(factory)
{
    [Fact]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "<Pending>")]
    public async Task Handle_ShouldAddConditionAndUpdateRuleVersion()
    {
        // Arrange
        Ecosystem ecosystem = new EcosystemBuilder().Build();
        Sensor sensor = new SensorBuilder().WithEcosystemId(ecosystem.Id).Build();
        Relay relay = new RelayBuilder().WithEcosystemId(ecosystem.Id).Build();
        AutomationRule rule = new AutomationRuleBuilder()
            .WithEcosystemId(ecosystem.Id)
            .WithRelayId(relay.Id)
            .Build();

        Guid initialVersion = rule.Version;

        DbContext.Set<Ecosystem>().Add(ecosystem);
        DbContext.Set<Sensor>().Add(sensor);
        DbContext.Set<Relay>().Add(relay);
        DbContext.Set<AutomationRule>().Add(rule);
        await DbContext.SaveChangesAsync();
        DbContext.ChangeTracker.Clear();

        var command = new AddConditionCommand
        {
            RuleId = rule.Id,
            SensorId = sensor.Id,
            Condition = Condition.Greater,
            Threshold = 25.5,
            Hysteresis = 0.5
        };

        // Act
        Result<Guid> result = await Sender.Send(command);

        // Assert
        result.IsSuccess.Should().BeTrue(result.Error?.Message ?? "No error message");

        AutomationRule? updatedRule = await DbContext.Set<AutomationRule>()
            .Include(r => r.Conditions)
            .AsNoTracking()
            .FirstOrDefaultAsync(r => r.Id == rule.Id);

        updatedRule.Should().NotBeNull();
        updatedRule!.Conditions.Should().HaveCount(1);
        updatedRule.Conditions.First().ConditionThreshold.Threshold.Should().Be(25.5);
        updatedRule.Version.Should().NotBe(initialVersion);
    }
}
