using Contracts.Results;
using FluentAssertions;
using Notification.Domain.Entities;

namespace Notification.Domain.UnitTests.Entities;

public class MaintenanceLogTests
{
    [Fact]
    public void Create_WithValidData_ReturnsSuccessAndInitializesProperties()
    {
        // Arrange
        var id = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var ecosystemId = Guid.NewGuid();
        DateTime actionDate = DateTime.UtcNow.AddMinutes(-10);
        var metrics = new Dictionary<string, double> { { "pH", 8.2 }, { "temp", 25.5 } };
        string notes = "  Cleaned the glass filter.   ";

        // Act
        Result<MaintenanceLog> result = MaintenanceLog.Create(id, userId, ecosystemId, actionDate, metrics, notes);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Id.Should().Be(id);
        result.Value.UserId.Should().Be(userId);
        result.Value.EcosystemId.Should().Be(ecosystemId);
        result.Value.ActionDate.Should().Be(actionDate);
        result.Value.Metrics.Should().ContainKey("pH").WhoseValue.Should().Be(8.2);
        result.Value.Metrics.Should().ContainKey("temp").WhoseValue.Should().Be(25.5);
        result.Value.Notes.Should().Be("Cleaned the glass filter.");
        result.Value.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
        result.Value.Version.Should().Be(Guid.Empty);
    }

    [Fact]
    public void Create_WithFutureActionDate_ReturnsFailure()
    {
        // Arrange
        var id = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var ecosystemId = Guid.NewGuid();
        DateTime futureDate = DateTime.UtcNow.AddMinutes(10);

        // Act
        Result<MaintenanceLog> result = MaintenanceLog.Create(
            id, userId, ecosystemId,
            futureDate, null, "Future maintenance log");

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("MaintenanceLog.Invalid");
        result.Error.Message.Should().Be("Action date cannot be in the future.");
    }

    [Fact]
    public void Create_WithNullMetrics_InitializesEmptyDictionary()
    {
        // Arrange
        var id = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var ecosystemId = Guid.NewGuid();
        DateTime actionDate = DateTime.UtcNow;

        // Act
        Result<MaintenanceLog> result = MaintenanceLog.Create(
            id, userId, ecosystemId,
            actionDate, null, "No metrics here");

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Metrics.Should().NotBeNull().And.BeEmpty();
    }
}
