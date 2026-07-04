// Ignore Spelling: Cron

namespace Control.Domain.UnitTests.Entities;

public class ScheduleTests
{
    private readonly ICronValidator _cronValidatorMock;

    public ScheduleTests()
    {
        _cronValidatorMock = Substitute.For<ICronValidator>();
    }

    [Fact]
    public void Create_WithValidData_ReturnsSuccess()
    {
        // Arrange
        var id = Guid.NewGuid();
        var ecosystemId = Guid.NewGuid();
        var relayId = Guid.NewGuid();
        string cronExpr = "0 8 * * *";
        double duration = 15.0;
        bool isFade = true;
        bool isEnabled = true;
        _cronValidatorMock.IsValid(cronExpr).Returns(true);

        // Act
        Result<Schedule> result = Schedule.Create(
            id, ecosystemId, relayId, cronExpr, _cronValidatorMock, duration, isFade, isEnabled);

        // Assert
        result.IsSuccess.Should().BeTrue();
        Schedule schedule = result.Value;
        schedule.Id.Should().Be(id);
        schedule.EcosystemId.Should().Be(ecosystemId);
        schedule.RelayId.Should().Be(relayId);
        schedule.CronExpression.Value.Should().Be(cronExpr);
        schedule.DurationMin.Should().Be(duration);
        schedule.IsFadeMode.Should().Be(isFade);
        schedule.IsEnabled.Should().Be(isEnabled);
    }

    [Fact]
    public void Create_WithInvalidCron_ReturnsFailure()
    {
        // Arrange
        string cronExpr = "invalid_cron";
        _cronValidatorMock.IsValid(cronExpr).Returns(false);

        // Act
        Result<Schedule> result = Schedule.Create(
            scheduleId: Guid.NewGuid(),
            ecosystemId: Guid.NewGuid(),
            relayId: Guid.NewGuid(),
            rawCronExpression: cronExpr,
            _cronValidatorMock,
            durationMin: 30.0,
            isFadeMode: false,
            isEnabled: true);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("CronSchedule.Invalid");
    }

    [Fact]
    public void Update_WithInvalidDuration_ReturnsFailureAndDoesNotIncrementVersion()
    {
        // Arrange
        Schedule schedule = new ScheduleBuilder().WithDurationMin(30.0).Build();
        Guid initialVersion = schedule.Version;

        // Act
        Result result = schedule.Update("0 12 * * *", _cronValidatorMock, -5.0, true, false);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Schedule.Invalid");
        result.Error.Message.Should().Contain("DurationMin must be strictly greater than zero");
        schedule.DurationMin.Should().Be(30.0);
        schedule.Version.Should().Be(initialVersion);
    }

    [Fact]
    public void Update_WithInvalidCron_ReturnsFailureAndDoesNotIncrementVersion()
    {
        // Arrange
        Schedule schedule = new ScheduleBuilder().WithCronExpression("0 12 * * *").Build();
        Guid initialVersion = schedule.Version;
        string newCron = "invalid_cron";
        _cronValidatorMock.IsValid(newCron).Returns(false);

        // Act
        Result result = schedule.Update(newCron, _cronValidatorMock, 45.0, true, false);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("CronSchedule.Invalid");
        schedule.CronExpression.Value.Should().Be("0 12 * * *");
        schedule.Version.Should().Be(initialVersion);
    }

    [Fact]
    public void Update_WithValidData_UpdatesPropertiesAndIncrementsVersion()
    {
        // Arrange
        Schedule schedule = new ScheduleBuilder()
            .WithCronExpression("0 12 * * *")
            .WithDurationMin(30.0)
            .WithIsFadeMode(false)
            .WithIsEnabled(true)
            .Build();
        Guid initialVersion = schedule.Version;
        string newCron = "0 18 * * *";
        _cronValidatorMock.IsValid(newCron).Returns(true);

        // Act
        Result result = schedule.Update(newCron, _cronValidatorMock, 45.0, true, false);

        // Assert
        result.IsSuccess.Should().BeTrue();
        schedule.CronExpression.Value.Should().Be(newCron);
        schedule.DurationMin.Should().Be(45.0);
        schedule.IsFadeMode.Should().BeTrue();
        schedule.IsEnabled.Should().BeFalse();
        schedule.Version.Should().NotBe(initialVersion);
    }

    [Fact]
    public void SetToggle_WithNewState_UpdatesIsEnabledAndIncrementsVersion()
    {
        // Arrange
        Schedule schedule = new ScheduleBuilder().WithIsEnabled(true).Build();
        Guid initialVersion = schedule.Version;

        // Act
        schedule.SetToggle(false);

        // Assert
        schedule.IsEnabled.Should().BeFalse();
        schedule.Version.Should().NotBe(initialVersion);
    }

    [Fact]
    public void SetToggle_WithSameState_DoesNotIncrementVersion()
    {
        // Arrange
        Schedule schedule = new ScheduleBuilder().WithIsEnabled(true).Build();
        Guid initialVersion = schedule.Version;

        // Act
        schedule.SetToggle(true);

        // Assert
        schedule.IsEnabled.Should().Be(true);
        schedule.Version.Should().Be(initialVersion);
    }
}
