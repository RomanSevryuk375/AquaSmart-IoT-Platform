// Ignore Spelling: Cron

namespace Control.Domain.UnitTests.ValueObjects;

public class CronScheduleTests
{
    private readonly ICronValidator _cronValidatorMock;

    public CronScheduleTests()
    {
        _cronValidatorMock = Substitute.For<ICronValidator>();
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void Create_WithEmptyOrWhitespaceCron_ReturnsFailure(string invalidCron)
    {
        // Act
        Result<CronSchedule> result = CronSchedule.Create(invalidCron, _cronValidatorMock);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("CronSchedule.Invalid");
        result.Error.Message.Should().Contain("Cron expression cannot be empty");
    }

    [Fact]
    public void Create_WithValidCronExpression_ReturnsSuccess()
    {
        // Arrange
        string cronExpr = "0 12 * * *";
        _cronValidatorMock.IsValid(cronExpr).Returns(true);

        // Act
        Result<CronSchedule> result = CronSchedule.Create(cronExpr, _cronValidatorMock);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Value.Should().Be(cronExpr);
        _cronValidatorMock.Received(1).IsValid(cronExpr);
    }

    [Fact]
    public void Create_WithInvalidCronExpression_ReturnsFailure()
    {
        // Arrange
        string cronExpr = "invalid_cron";
        _cronValidatorMock.IsValid(cronExpr).Returns(false);

        // Act
        Result<CronSchedule> result = CronSchedule.Create(cronExpr, _cronValidatorMock);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("CronSchedule.Invalid");
        result.Error.Message.Should().Contain("Invalid cron expression");
        _cronValidatorMock.Received(1).IsValid(cronExpr);
    }

    [Fact]
    public void Load_BypassesValidation_ReturnsCronSchedule()
    {
        // Arrange
        string cronExpr = "unvalidated_cron";

        // Act
        var cron = CronSchedule.Load(cronExpr);

        // Assert
        cron.Value.Should().Be(cronExpr);
        cron.ToString().Should().Be(cronExpr);
    }
}
