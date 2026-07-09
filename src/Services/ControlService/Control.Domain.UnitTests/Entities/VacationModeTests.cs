namespace Control.Domain.UnitTests.Entities;

public class VacationModeTests
{
    [Fact]
    public void Create_WithValidData_ReturnsSuccess()
    {
        // Arrange
        var id = Guid.NewGuid();
        var ecosystemId = Guid.NewGuid();
        DateTime start = DateTime.UtcNow.AddDays(1);
        DateTime end = DateTime.UtcNow.AddDays(5);
        bool isActive = true;
        double feed = 100.0;

        // Act
        Result<VacationMode> result = VacationMode.Create(
            id, ecosystemId, start, end, isActive, feed);

        // Assert
        result.IsSuccess.Should().BeTrue();
        VacationMode vacation = result.Value;
        vacation.Id.Should().Be(id);
        vacation.EcosystemId.Should().Be(ecosystemId);
        vacation.DateRange.StartDate.Should().Be(start);
        vacation.DateRange.EndDate.Should().Be(end);
        vacation.IsActive.Should().Be(isActive);
        vacation.CalculatedFeed.Should().Be(feed);
    }

    [Fact]
    public void Create_WithInvalidDateRange_ReturnsFailure()
    {
        // Arrange
        DateTime start = DateTime.UtcNow.AddDays(5);
        DateTime end = DateTime.UtcNow.AddDays(1);

        // Act
        Result<VacationMode> result = VacationMode.Create(
            vacationModeId: Guid.NewGuid(),
            ecosystemId: Guid.NewGuid(),
            start,
            end,
            isActive: false,
            calculatedFeed: 100.0);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("DateRange.Invalid");
    }

    [Fact]
    public void SetActive_WithNewStatus_UpdatesIsActiveAndIncrementsVersion()
    {
        // Arrange
        VacationMode vacation = new VacationModeBuilder().WithIsActive(false).Build();
        Guid initialVersion = vacation.Version;

        // Act
        vacation.SetActive(true);

        // Assert
        vacation.IsActive.Should().BeTrue();
        vacation.Version.Should().NotBe(initialVersion);
    }

    [Fact]
    public void SetActive_WithSameStatus_DoesNotIncrementVersion()
    {
        // Arrange
        VacationMode vacation = new VacationModeBuilder().WithIsActive(false).Build();
        Guid initialVersion = vacation.Version;

        // Act
        vacation.SetActive(false);

        // Assert
        vacation.IsActive.Should().BeFalse();
        vacation.Version.Should().Be(initialVersion);
    }

    [Fact]
    public void SetTiming_WithValidRange_UpdatesRangeAndIncrementsVersion()
    {
        // Arrange
        VacationMode vacation = new VacationModeBuilder().Build();
        Guid initialVersion = vacation.Version;
        DateTime newStart = DateTime.UtcNow.AddDays(2);
        DateTime newEnd = DateTime.UtcNow.AddDays(8);

        // Act
        Result result = vacation.SetTiming(newStart, newEnd);

        // Assert
        result.IsSuccess.Should().BeTrue();
        vacation.DateRange.StartDate.Should().Be(newStart);
        vacation.DateRange.EndDate.Should().Be(newEnd);
        vacation.Version.Should().NotBe(initialVersion);
    }

    [Fact]
    public void SetTiming_WithInvalidRange_ReturnsFailureAndDoesNotIncrementVersion()
    {
        // Arrange
        VacationMode vacation = new VacationModeBuilder().Build();
        DateRange initialRange = vacation.DateRange;
        Guid initialVersion = vacation.Version;

        // Act
        Result result = vacation.SetTiming(DateTime.UtcNow.AddDays(5), DateTime.UtcNow.AddDays(2));

        // Assert
        result.IsFailure.Should().BeTrue();
        vacation.DateRange.Should().Be(initialRange);
        vacation.Version.Should().Be(initialVersion);
    }

    [Fact]
    public void SetFeedSize_WithValidFeed_UpdatesFeedAndIncrementsVersion()
    {
        // Arrange
        VacationMode vacation = new VacationModeBuilder().WithCalculatedFeed(50.0).Build();
        Guid initialVersion = vacation.Version;

        // Act
        Result result = vacation.SetFeedSize(75.5);

        // Assert
        result.IsSuccess.Should().BeTrue();
        vacation.CalculatedFeed.Should().Be(75.5);
        vacation.Version.Should().NotBe(initialVersion);
    }

    [Fact]
    public void SetFeedSize_WithNegativeFeed_ReturnsFailureAndDoesNotIncrementVersion()
    {
        // Arrange
        VacationMode vacation = new VacationModeBuilder().WithCalculatedFeed(50.0).Build();
        Guid initialVersion = vacation.Version;

        // Act
        Result result = vacation.SetFeedSize(-10.0);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("VacationMode.Invalid");
        vacation.CalculatedFeed.Should().Be(50.0);
        vacation.Version.Should().Be(initialVersion);
    }
}
