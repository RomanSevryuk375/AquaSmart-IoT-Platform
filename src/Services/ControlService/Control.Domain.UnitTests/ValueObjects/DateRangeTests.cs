namespace Control.Domain.UnitTests.ValueObjects;

public class DateRangeTests
{
    [Fact]
    public void Create_WithValidDateRange_ReturnsSuccess()
    {
        // Arrange
        DateTime start = DateTime.UtcNow;
        DateTime end = start.AddDays(5);

        // Act
        Result<DateRange> result = DateRange.Create(start, end);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.StartDate.Should().Be(start);
        result.Value.EndDate.Should().Be(end);
    }

    [Fact]
    public void Create_WithStartEqualEnd_ReturnsFailure()
    {
        // Arrange
        DateTime start = DateTime.UtcNow;
        DateTime end = start;

        // Act
        Result<DateRange> result = DateRange.Create(start, end);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("DateRange.Invalid");
        result.Error.Message.Should().Contain("StartDate must be strictly before EndDate");
    }

    [Fact]
    public void Create_WithStartAfterEnd_ReturnsFailure()
    {
        // Arrange
        DateTime start = DateTime.UtcNow;
        DateTime end = start.AddDays(-1);

        // Act
        Result<DateRange> result = DateRange.Create(start, end);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("DateRange.Invalid");
        result.Error.Message.Should().Contain("StartDate must be strictly before EndDate");
    }

    [Fact]
    public void Parse_WithValidString_ReturnsExpectedDateRange()
    {
        // Arrange
        var start = new DateTime(2026, 7, 4, 12, 0, 0, DateTimeKind.Utc);
        var end = new DateTime(2026, 7, 14, 12, 0, 0, DateTimeKind.Utc);
        string dbValue = $"{start}_{end}";

        // Act
        var dateRange = DateRange.Parse(dbValue);

        // Assert
        dateRange.Should().NotBeNull();
        dateRange.StartDate.Should().BeCloseTo(start, TimeSpan.FromSeconds(1));
        dateRange.EndDate.Should().BeCloseTo(end, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void ToString_ReturnsExpectedFormattedString()
    {
        // Arrange
        DateTime start = DateTime.UtcNow;
        DateTime end = start.AddDays(2);
        DateRange dateRange = DateRange.Create(start, end).Value;

        // Act
        string str = dateRange.ToString();

        // Assert
        str.Should().Be($"{start}_{end}");
    }
}
