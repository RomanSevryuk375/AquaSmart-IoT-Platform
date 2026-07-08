using Contracts.Results;
using FluentAssertions;
using Notification.Application.Features.MaintenanceLogs.Commands.CreateMaintenanceLog;
using Notification.Domain.Entities;
using Notification.Domain.Interfaces;
using NSubstitute;

namespace Notification.Application.UnitTests.Features.MaintenanceLogs.Commands.CreateMaintenanceLog;

public class CreateMaintenanceLogHandlerTests
{
    private readonly IMaintenanceLogRepository _logRepoMock = Substitute.For<IMaintenanceLogRepository>();
    private readonly CreateMaintenanceLogHandler _handler;

    public CreateMaintenanceLogHandlerTests()
    {
        _handler = new CreateMaintenanceLogHandler(_logRepoMock);
    }

    [Fact]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "<Pending>")]
    public async Task Handle_WhenValidCommand_CreatesMaintenanceLogAndReturnsSuccess()
    {
        // Arrange
        var command = new CreateMaintenanceLogCommand
        {
            UserId = Guid.NewGuid(),
            EcosystemId = Guid.NewGuid(),
            ActionDate = DateTime.UtcNow,
            Metrics = new Dictionary<string, double> { { "pH", 7.2 } },
            Notes = "Added some buffering agent"
        };

        // Act
        Result<Guid> result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeEmpty();

        await _logRepoMock.Received(1).AddAsync(
            Arg.Is<MaintenanceLog>(l =>
                l.UserId == command.UserId &&
                l.EcosystemId == command.EcosystemId &&
                l.ActionDate == command.ActionDate &&
                Math.Abs(l.Metrics["pH"] - 7.2) < 0.001 &&
                l.Notes == "Added some buffering agent"),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "<Pending>")]
    public async Task Handle_WhenFutureActionDate_ReturnsValidationFailure()
    {
        // Arrange
        var command = new CreateMaintenanceLogCommand
        {
            UserId = Guid.NewGuid(),
            EcosystemId = Guid.NewGuid(),
            ActionDate = DateTime.UtcNow.AddHours(2),
            Metrics = new Dictionary<string, double>(),
            Notes = "Test"
        };

        // Act
        Result<Guid> result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("MaintenanceLog.Invalid");
        result.Error.Message.Should().Be("Action date cannot be in the future.");

        await _logRepoMock.DidNotReceive().AddAsync(Arg.Any<MaintenanceLog>(), Arg.Any<CancellationToken>());
    }
}
