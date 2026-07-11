using Control.Application.DTOs.AutomationRule;
using Control.Application.Features.AutomationRules.Commands.CreateRule;

namespace Control.Application.UnitTests.Features.AutomationRules;

public class CreateRuleHandlerTests
{
    private readonly IAutomationRuleRepository _ruleRepoMock = Substitute.For<IAutomationRuleRepository>();
    private readonly IHardwareValidator _hardwareValidatorMock = Substitute.For<IHardwareValidator>();
    private readonly CreateRuleHandler _handler;

    public CreateRuleHandlerTests()
    {
        _handler = new CreateRuleHandler(_ruleRepoMock, _hardwareValidatorMock);
    }

    [Fact]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "<Pending>")]
    public async Task Handle_WithValidCommand_CreatesRuleAndReturnsSuccess()
    {
        // Arrange
        var sensorId1 = Guid.NewGuid();
        var sensorId2 = Guid.NewGuid();
        var relayId = Guid.NewGuid();

        var command = new CreateRuleCommand
        {
            EcosystemId = Guid.NewGuid(),
            RelayId = relayId,
            Name = "Temperature Rule",
            Operator = Operator.AND,
            Action = RuleAction.SwitchOn,
            IsActive = true,
            Conditions =
            [
                new RuleConditionRequestDto { SensorId = sensorId1, Condition = Condition.Greater, Threshold = 25.0, Hysteresis = 1.0 },
                new RuleConditionRequestDto { SensorId = sensorId2, Condition = Condition.Less, Threshold = 15.0, Hysteresis = 0.5 }
            ]
        };

        var expectedRuleId = Guid.NewGuid();

        _hardwareValidatorMock.ValidateAssignmentAsync(Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success());

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
                r.IsActive == command.IsActive &&
                r.Conditions.Count == 2),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "<Pending>")]
    public async Task Handle_WhenHardwareValidationFails_ReturnsFailureAndDoesNotSave()
    {
        // Arrange
        var sensorId1 = Guid.NewGuid();
        var relayId = Guid.NewGuid();

        var command = new CreateRuleCommand
        {
            EcosystemId = Guid.NewGuid(),
            RelayId = relayId,
            Name = "Temperature Rule",
            Operator = Operator.AND,
            Action = RuleAction.SwitchOn,
            IsActive = true,
            Conditions =
            [
                new RuleConditionRequestDto { SensorId = sensorId1, Condition = Condition.Greater, Threshold = 25.0, Hysteresis = 1.0 }
            ]
        };

        var expectedError = Error.Conflict("Hardware.Mismatch", "Sensor and Relay do not belong to the same controller.");

        _hardwareValidatorMock.ValidateAssignmentAsync(Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns(Result.Failure(expectedError));

        // Act
        Result<Guid> result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(expectedError);

        await _ruleRepoMock.DidNotReceive().AddAsync(Arg.Any<AutomationRule>(), Arg.Any<CancellationToken>());
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
