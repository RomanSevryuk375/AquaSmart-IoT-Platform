using Control.Application.Features.AutomationRules.Commands.CreateRule;

namespace Control.Application.UnitTests.Features.AutomationRules;

public class CreateRuleHandlerTests
{
    private readonly IAutomationRuleRepository _ruleRepoMock = Substitute.For<IAutomationRuleRepository>();
    private readonly CreateRuleHandler _handler;

    public CreateRuleHandlerTests()
    {
        _handler = new CreateRuleHandler(_ruleRepoMock);
    }

    [Fact]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "<Pending>")]
    public async Task Handle_WithValidCommand_CreatesRuleAndReturnsSuccess()
    {
        // Arrange
        var command = new CreateRuleCommand
        {
            EcosystemId = Guid.NewGuid(),
            RelayId = Guid.NewGuid(),
            Name = "Temperature Rule",
            Operator = Operator.AND,
            Action = RuleAction.SwitchOn,
            IsActive = true
        };

        var expectedRuleId = Guid.NewGuid();
        _ruleRepoMock.AddAsync(Arg.Any<AutomationRule>(), Arg.Any<CancellationToken>())
            .Returns(expectedRuleId);

        // Act
        Result<Guid> result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(expectedRuleId);

        await _ruleRepoMock.Received(1).AddAsync(
            Arg.Is<AutomationRule>(r =>
                r.EcosystemId == command.EcosystemId &&
                r.RelayId == command.RelayId &&
                r.Name.Value == command.Name &&
                r.Operator == command.Operator &&
                r.Action == command.Action &&
                r.IsActive == command.IsActive),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "<Pending>")]
    public async Task Handle_WithInvalidCommandName_ReturnsFailure()
    {
        // Arrange
        var command = new CreateRuleCommand
        {
            EcosystemId = Guid.NewGuid(),
            RelayId = Guid.NewGuid(),
            Name = "",
            Operator = Operator.AND,
            Action = RuleAction.SwitchOn,
            IsActive = true
        };

        // Act
        Result<Guid> result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Name.Invalid");

        await _ruleRepoMock.DidNotReceive().AddAsync(Arg.Any<AutomationRule>(), Arg.Any<CancellationToken>());
    }
}
