namespace Control.Domain.UnitTests.Entities;

public class RuleConditionTests
{
    [Fact]
    public void Create_WithValidData_ReturnsSuccess()
    {
        // Arrange
        var id = Guid.NewGuid();
        var ruleId = Guid.NewGuid();
        var sensorId = Guid.NewGuid();
        Condition condition = Condition.Greater;
        double threshold = 25.0;
        double hysteresis = 0.5;

        // Act
        Result<RuleCondition> result = RuleCondition.Create(
            id, ruleId, sensorId, condition, threshold, hysteresis);

        // Assert
        result.IsSuccess.Should().BeTrue();
        RuleCondition ruleCondition = result.Value;
        ruleCondition.Id.Should().Be(id);
        ruleCondition.AutomationRuleId.Should().Be(ruleId);
        ruleCondition.SensorId.Should().Be(sensorId);
        ruleCondition.Condition.Should().Be(condition);
        ruleCondition.ConditionThreshold.Threshold.Should().Be(threshold);
        ruleCondition.ConditionThreshold.Hysteresis.Should().Be(hysteresis);
    }

    [Fact]
    public void Create_WithInvalidHysteresis_ReturnsFailure()
    {
        // Act
        Result<RuleCondition> result = RuleCondition.Create(
            Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Condition.Greater, 25.0, -1.0);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("ConditionThreshold.Invalid");
    }

    [Fact]
    public void Update_WithValidData_UpdatesPropertiesAndReturnsSuccess()
    {
        // Arrange
        RuleCondition ruleCondition = new RuleConditionBuilder()
            .WithCondition(Condition.Greater)
            .WithThreshold(25.0)
            .WithHysteresis(0.5)
            .Build();

        // Act
        Result result = ruleCondition.Update(Condition.Less, 20.0, 0.2);

        // Assert
        result.IsSuccess.Should().BeTrue();
        ruleCondition.Condition.Should().Be(Condition.Less);
        ruleCondition.ConditionThreshold.Threshold.Should().Be(20.0);
        ruleCondition.ConditionThreshold.Hysteresis.Should().Be(0.2);
    }

    [Fact]
    public void Update_WithInvalidHysteresis_ReturnsFailureAndDoesNotChangeState()
    {
        // Arrange
        RuleCondition ruleCondition = new RuleConditionBuilder()
            .WithCondition(Condition.Greater)
            .WithThreshold(25.0)
            .WithHysteresis(0.5)
            .Build();

        // Act
        Result result = ruleCondition.Update(Condition.Less, 20.0, -0.5);

        // Assert
        result.IsFailure.Should().BeTrue();
        ruleCondition.Condition.Should().Be(Condition.Greater);
        ruleCondition.ConditionThreshold.Threshold.Should().Be(25.0);
        ruleCondition.ConditionThreshold.Hysteresis.Should().Be(0.5);
    }

    [Fact]
    public void SetConditionType_WithNewCondition_UpdatesCondition()
    {
        // Arrange
        RuleCondition ruleCondition = new RuleConditionBuilder()
            .WithCondition(Condition.Greater).Build();

        // Act
        ruleCondition.SetConditionType(Condition.Equal);

        // Assert
        ruleCondition.Condition.Should().Be(Condition.Equal);
    }

    [Fact]
    public void SetConditionType_WithSameCondition_DoesNotChangeCondition()
    {
        // Arrange
        RuleCondition ruleCondition = new RuleConditionBuilder()
            .WithCondition(Condition.Greater).Build();

        // Act
        ruleCondition.SetConditionType(Condition.Greater);

        // Assert
        ruleCondition.Condition.Should().Be(Condition.Greater);
    }

    [Theory]
    // Greater (Threshold 25.0, Hysteresis 0.5 -> triggers true > 25.5, false < 24.5, else null)
    [InlineData(Condition.Greater, 25.0, 0.5, 26.0, true)]
    [InlineData(Condition.Greater, 25.0, 0.5, 24.0, false)]
    [InlineData(Condition.Greater, 25.0, 0.5, 25.0, null)]
    // Less (Threshold 25.0, Hysteresis 0.5 -> triggers true < 24.5, false > 25.5, else null)
    [InlineData(Condition.Less, 25.0, 0.5, 24.0, true)]
    [InlineData(Condition.Less, 25.0, 0.5, 26.0, false)]
    [InlineData(Condition.Less, 25.0, 0.5, 25.0, null)]
    public void Evaluate_ReturnsExpectedResultBasedOnEvaluatorStrategies(
        Condition condition,
        double threshold,
        double hysteresis,
        double currentValue,
        bool? expectedResult)
    {
        // Arrange
        RuleCondition ruleCondition = new RuleConditionBuilder()
            .WithCondition(condition)
            .WithThreshold(threshold)
            .WithHysteresis(hysteresis)
            .Build();

        // Act
        bool? result = ruleCondition.Evaluate(currentValue);

        // Assert
        result.Should().Be(expectedResult);
    }
}
