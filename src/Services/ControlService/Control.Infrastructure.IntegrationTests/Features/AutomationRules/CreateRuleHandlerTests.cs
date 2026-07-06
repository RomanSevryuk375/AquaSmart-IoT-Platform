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

        DbContext.Set<Ecosystem>().Add(ecosystem);
        DbContext.Set<Relay>().Add(relay);

        var command = new CreateRuleCommand
        {
            EcosystemId = ecosystem.Id,
            RelayId = relay.Id,
            Name = "Temperature Control",
            Operator = Operator.AND,
            Action = RuleAction.SwitchOn,
            IsActive = true
        };

        // Act
        Result<Guid> result = await Sender.Send(command);

        // Assert
        result.IsSuccess.Should().BeTrue(result.Error?.Message ?? "No error message");

        Guid ruleId = result.Value;

        AutomationRule? rule = await DbContext.Set<AutomationRule>()
            .AsNoTracking()
            .FirstOrDefaultAsync(r => r.Id == ruleId);

        rule.Should().NotBeNull();
        rule!.Name.Value.Should().Be("Temperature Control");

        List<OutboxMessage> outboxMessages = await DbContext.Set<OutboxMessage>()
            .AsNoTracking()
            .ToListAsync();

        outboxMessages.Should().NotBeEmpty();
    }
}
