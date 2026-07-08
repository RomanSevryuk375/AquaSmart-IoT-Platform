using Contracts.Results;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Notification.Application.Features.MaintenanceLogs.Commands.CreateMaintenanceLog;
using Notification.Domain.Entities;
using Notification.Infrastructure.IntegrationTests.Infrastructure;
using Notification.TestShared.Builders;

namespace Notification.Infrastructure.IntegrationTests.Features.MaintenanceLogs;

public class CreateMaintenanceLogHandlerTests(IntegrationTestWebAppFactory factory)
    : BaseIntegrationTest(factory)
{
    [Fact]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "<Pending>")]
    public async Task Handle_ShouldSaveMetricsToJsonbColumnCorrectly_WhenCommandContainsComplexMetrics()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var ecosystemId = Guid.NewGuid();

        User user = new UserBuilder()
            .WithId(userId)
            .WithEmail("user@example.com")
            .Build();

        Ecosystem ecosystem = new EcosystemBuilder()
            .WithId(ecosystemId)
            .WithUserId(userId)
            .WithName("Test Ecosystem")
            .Build();

        DbContext.Set<User>().Add(user);
        DbContext.Set<Ecosystem>().Add(ecosystem);
        await DbContext.SaveChangesAsync();
        DbContext.ChangeTracker.Clear();

        UserContext.UserId = userId;

        var command = new CreateMaintenanceLogCommand
        {
            UserId = userId,
            EcosystemId = ecosystemId,
            ActionDate = DateTime.UtcNow,
            Metrics = new Dictionary<string, double>
            {
                { "Ph", 7.5 },
                { "Temp", 25.4 },
                { "Salinity", 1.024 }
            },
            Notes = "Water parameters look stable."
        };

        // Act
        Result<Guid> result = await Sender.Send(command);

        // Assert
        result.IsSuccess.Should().BeTrue();
        Guid logId = result.Value;

        MaintenanceLog? persistedLog = await DbContext.Set<MaintenanceLog>()
            .AsNoTracking()
            .FirstOrDefaultAsync(l => l.Id == logId);

        persistedLog.Should().NotBeNull();
        persistedLog!.UserId.Should().Be(userId);
        persistedLog.EcosystemId.Should().Be(ecosystemId);
        persistedLog.Notes.Should().Be("Water parameters look stable.");

        persistedLog.Metrics.Should().NotBeNull();
        persistedLog.Metrics.Should().HaveCount(3);
        persistedLog.Metrics.Should().ContainKey("Ph").WhoseValue.Should().Be(7.5);
        persistedLog.Metrics.Should().ContainKey("Temp").WhoseValue.Should().Be(25.4);
        persistedLog.Metrics.Should().ContainKey("Salinity").WhoseValue.Should().Be(1.024);
    }
}
