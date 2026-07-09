namespace Control.Domain.UnitTests.Entities;

public class AutomationRuleTests
{
    [Fact]
    public void Create_WithValidData_ReturnsSuccess()
    {
        // Arrange
        var id = Guid.NewGuid();
        var ecosystemId = Guid.NewGuid();
        string name = "High Temperature Protection";
        var relayId = Guid.NewGuid();
        Operator op = Operator.AND;
        RuleAction action = RuleAction.SwitchOn;
        bool isActive = true;

        // Act
        Result<AutomationRule> result = AutomationRule.Create(
            id, ecosystemId, name, relayId, op, action, isActive);

        // Assert
        result.IsSuccess.Should().BeTrue();
        AutomationRule rule = result.Value;
        rule.Id.Should().Be(id);
        rule.EcosystemId.Should().Be(ecosystemId);
        rule.Name.Value.Should().Be(name);
        rule.RelayId.Should().Be(relayId);
        rule.Operator.Should().Be(op);
        rule.Action.Should().Be(action);
        rule.IsActive.Should().Be(isActive);
        rule.Conditions.Should().BeEmpty();
    }

    [Fact]
    public void Create_WithInvalidName_ReturnsFailure()
    {
        // Act
        Result<AutomationRule> result = AutomationRule.Create(
            ruleId: Guid.NewGuid(),
            ecosystemId: Guid.NewGuid(),
            rawName: "",
            relayId: Guid.NewGuid(),
            Operator.AND,
            RuleAction.SwitchOn,
            isActive: true);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Name.Invalid");
    }

    [Fact]
    public void Update_WithValidData_UpdatesPropertiesAndIncrementsVersion()
    {
        // Arrange
        AutomationRule rule = new AutomationRuleBuilder()
            .WithName("Old Name")
            .WithOperator(Operator.AND)
            .WithAction(RuleAction.SwitchOn)
            .Build();
        Guid initialVersion = rule.Version;
        var newRelayId = Guid.NewGuid();

        // Act
        Result result = rule.Update("New Name", newRelayId, Operator.OR, RuleAction.SwitchOff);

        // Assert
        result.IsSuccess.Should().BeTrue();
        rule.Name.Value.Should().Be("New Name");
        rule.RelayId.Should().Be(newRelayId);
        rule.Operator.Should().Be(Operator.OR);
        rule.Action.Should().Be(RuleAction.SwitchOff);
        rule.Version.Should().NotBe(initialVersion);
    }

    [Fact]
    public void Update_WithInvalidName_ReturnsFailureAndDoesNotIncrementVersion()
    {
        // Arrange
        AutomationRule rule = new AutomationRuleBuilder().WithName("Old Name").Build();
        Guid initialVersion = rule.Version;

        // Act
        Result result = rule.Update("", Guid.NewGuid(), Operator.OR, RuleAction.SwitchOff);

        // Assert
        result.IsFailure.Should().BeTrue();
        rule.Name.Value.Should().Be("Old Name");
        rule.Version.Should().Be(initialVersion);
    }

    [Fact]
    public void SetAction_WithNewAction_UpdatesActionAndIncrementsVersion()
    {
        // Arrange
        AutomationRule rule = new AutomationRuleBuilder().WithAction(RuleAction.SwitchOn).Build();
        Guid initialVersion = rule.Version;

        // Act
        rule.SetAction(RuleAction.SwitchOff);

        // Assert
        rule.Action.Should().Be(RuleAction.SwitchOff);
        rule.Version.Should().NotBe(initialVersion);
    }

    [Fact]
    public void SetAction_WithSameAction_DoesNotIncrementVersion()
    {
        // Arrange
        AutomationRule rule = new AutomationRuleBuilder().WithAction(RuleAction.SwitchOn).Build();
        Guid initialVersion = rule.Version;

        // Act
        rule.SetAction(RuleAction.SwitchOn);

        // Assert
        rule.Action.Should().Be(RuleAction.SwitchOn);
        rule.Version.Should().Be(initialVersion);
    }

    [Fact]
    public void SetIsActive_WithNewState_UpdatesIsActiveAndIncrementsVersion()
    {
        // Arrange
        AutomationRule rule = new AutomationRuleBuilder().WithIsActive(true).Build();
        Guid initialVersion = rule.Version;

        // Act
        rule.SetIsActive(false);

        // Assert
        rule.IsActive.Should().BeFalse();
        rule.Version.Should().NotBe(initialVersion);
    }

    [Fact]
    public void SetIsActive_WithSameState_DoesNotIncrementVersion()
    {
        // Arrange
        AutomationRule rule = new AutomationRuleBuilder().WithIsActive(true).Build();
        Guid initialVersion = rule.Version;

        // Act
        rule.SetIsActive(true);

        // Assert
        rule.IsActive.Should().BeTrue();
        rule.Version.Should().Be(initialVersion);
    }

    [Fact]
    public void AddCondition_WithUniqueSensor_AddsConditionAndIncrementsVersion()
    {
        // Arrange
        AutomationRule rule = new AutomationRuleBuilder().Build();
        Guid initialVersion = rule.Version;
        RuleCondition condition = new RuleConditionBuilder().WithSensorId(Guid.NewGuid()).Build();

        // Act
        Result result = rule.AddCondition(condition);

        // Assert
        result.IsSuccess.Should().BeTrue();
        rule.Conditions.Should().ContainSingle().Which.Should().Be(condition);
        rule.Version.Should().NotBe(initialVersion);
    }

    [Fact]
    public void AddCondition_WithDuplicateSensor_ReturnsConflictAndDoesNotIncrementVersion()
    {
        // Arrange
        var sensorId = Guid.NewGuid();
        RuleCondition firstCondition = new RuleConditionBuilder().WithSensorId(sensorId).Build();
        AutomationRule rule = new AutomationRuleBuilder().WithCondition(firstCondition).Build();
        Guid initialVersion = rule.Version;

        RuleCondition secondCondition = new RuleConditionBuilder().WithSensorId(sensorId).Build();

        // Act
        Result result = rule.AddCondition(secondCondition);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("RuleCondition.Conflict");
        rule.Conditions.Should().ContainSingle().Which.Should().Be(firstCondition);
        rule.Version.Should().Be(initialVersion);
    }

    [Fact]
    public void RemoveCondition_WithExistingCondition_RemovesConditionAndIncrementsVersion()
    {
        // Arrange
        RuleCondition condition = new RuleConditionBuilder().Build();
        AutomationRule rule = new AutomationRuleBuilder().WithCondition(condition).Build();
        Guid initialVersion = rule.Version;

        // Act
        Result result = rule.RemoveCondition(condition);

        // Assert
        result.IsSuccess.Should().BeTrue();
        rule.Conditions.Should().BeEmpty();
        rule.Version.Should().NotBe(initialVersion);
    }

    [Fact]
    public void RemoveCondition_WithNonExistingCondition_ReturnsNotFoundAndDoesNotIncrementVersion()
    {
        // Arrange
        AutomationRule rule = new AutomationRuleBuilder().Build();
        Guid initialVersion = rule.Version;
        RuleCondition condition = new RuleConditionBuilder().Build();

        // Act
        Result result = rule.RemoveCondition(condition);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("RuleCondition.NotFound");
        rule.Version.Should().Be(initialVersion);
    }

    [Fact]
    public void EvaluateTargetState_WhenRuleIsInactive_ReturnsNull()
    {
        // Arrange
        RuleCondition condition = new RuleConditionBuilder().Build();
        AutomationRule rule = new AutomationRuleBuilder()
            .WithIsActive(false)
            .WithCondition(condition)
            .Build();

        var sensorData = new Dictionary<Guid, double> { { condition.SensorId, 30.0 } };

        // Act
        bool? targetState = rule.EvaluateTargetState(sensorData);

        // Assert
        targetState.Should().BeNull();
    }

    [Fact]
    public void EvaluateTargetState_WhenConditionsAreEmpty_ReturnsNull()
    {
        // Arrange
        AutomationRule rule = new AutomationRuleBuilder()
            .WithIsActive(true)
            .Build();

        var sensorData = new Dictionary<Guid, double>();

        // Act
        bool? targetState = rule.EvaluateTargetState(sensorData);

        // Assert
        targetState.Should().BeNull();
    }

    [Fact]
    public void EvaluateTargetState_WhenNoSensorMatchesCondition_ReturnsNull()
    {
        // Arrange
        RuleCondition condition = new RuleConditionBuilder().WithSensorId(Guid.NewGuid()).Build();
        AutomationRule rule = new AutomationRuleBuilder()
            .WithIsActive(true)
            .WithCondition(condition)
            .Build();

        var sensorData = new Dictionary<Guid, double> { { Guid.NewGuid(), 30.0 } };

        // Act
        bool? targetState = rule.EvaluateTargetState(sensorData);

        // Assert
        targetState.Should().BeNull();
    }

    [Theory]
    // SwitchOn Action:
    // If Rule is triggered (true) -> return true (SwitchOn)
    // If Rule is not triggered (false) -> return false (SwitchOff)
    [InlineData(RuleAction.SwitchOn, 26.0, true)]
    [InlineData(RuleAction.SwitchOn, 24.0, false)]
    [InlineData(RuleAction.SwitchOn, 25.0, null)]
    // SwitchOff Action:
    // If Rule is triggered (true) -> return true
    // If Rule is not triggered (false) -> return false
    [InlineData(RuleAction.SwitchOff, 26.0, true)]
    [InlineData(RuleAction.SwitchOff, 24.0, false)]
    [InlineData(RuleAction.SwitchOff, 25.0, null)]
    public void EvaluateTargetState_SingleCondition_EvaluatesCorrectly(
        RuleAction action,
        double currentValue,
        bool? expectedTargetState)
    {
        // Arrange
        var sensorId = Guid.NewGuid();
        RuleCondition condition = new RuleConditionBuilder()
            .WithSensorId(sensorId)
            .WithCondition(Condition.Greater)
            .WithThreshold(25.0)
            .WithHysteresis(0.5)
            .Build();

        AutomationRule rule = new AutomationRuleBuilder()
            .WithIsActive(true)
            .WithAction(action)
            .WithOperator(Operator.AND)
            .WithCondition(condition)
            .Build();

        var sensorData = new Dictionary<Guid, double> { { sensorId, currentValue } };

        // Act
        bool? targetState = rule.EvaluateTargetState(sensorData);

        // Assert
        targetState.Should().Be(expectedTargetState);
    }

    [Fact]
    public void EvaluateTargetState_MultipleConditionsAndOperator_EvaluatesCorrectly()
    {
        // Arrange
        var tempSensorId = Guid.NewGuid();
        var phSensorId = Guid.NewGuid();

        RuleCondition tempCondition = new RuleConditionBuilder()
            .WithSensorId(tempSensorId)
            .WithCondition(Condition.Greater)
            .WithThreshold(25.0)
            .WithHysteresis(0.5)
            .Build();

        RuleCondition phCondition = new RuleConditionBuilder()
            .WithSensorId(phSensorId)
            .WithCondition(Condition.Less)
            .WithThreshold(8.0)
            .WithHysteresis(0.1)
            .Build();

        AutomationRule rule = new AutomationRuleBuilder()
            .WithIsActive(true)
            .WithAction(RuleAction.SwitchOn)
            .WithOperator(Operator.AND)
            .WithConditions([tempCondition, phCondition])
            .Build();

        var sensorDataSatisfied = new Dictionary<Guid, double>
        {
            { tempSensorId, 26.0 },
            { phSensorId, 7.5 }
        };
        rule.EvaluateTargetState(sensorDataSatisfied).Should().BeTrue();

        var sensorDataFailed = new Dictionary<Guid, double>
        {
            { tempSensorId, 26.0 },
            { phSensorId, 8.5 }
        };
        rule.EvaluateTargetState(sensorDataFailed).Should().BeFalse();
    }
}
