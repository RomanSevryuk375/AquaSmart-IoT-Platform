using Contracts.Enums;
using Contracts.Results;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Telemetry.Application.Features.BackgroundJobs.Commands.CheckSensorState;
using Telemetry.Domain.Entities;
using Telemetry.Infrastructure.IntegrationTests.Infrastructure;
using Telemetry.Infrastructure.Persistence.Outbox;
using Telemetry.TestShared.Builders;

namespace Telemetry.Infrastructure.IntegrationTests.Features;

public class CheckSensorStateHandlerTests(IntegrationTestWebAppFactory factory)
    : BaseIntegrationTest(factory)
{
    [Fact]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "<Pending>")]
    public async Task Handle_WhenSensorIsDelayed_MarksAsNoDataAndSavesToOutbox()
    {
        // Arrange
        Ecosystem ecosystem = new EcosystemBuilder().Build();

        Sensor delayedSensor = new SensorBuilder()
            .WithEcosystemId(ecosystem.Id)
            .WithState(SensorState.Active)
            .WithUpdatedAt(DateTime.UtcNow.AddMinutes(-10))
            .Build();

        Sensor activeSensor = new SensorBuilder()
            .WithEcosystemId(ecosystem.Id)
            .WithState(SensorState.Active)
            .WithUpdatedAt(DateTime.UtcNow.AddMinutes(-2))
            .Build();

        DbContext.Ecosystems.Add(ecosystem);
        DbContext.Sensors.AddRange(delayedSensor, activeSensor);
        await DbContext.SaveChangesAsync();
        DbContext.ChangeTracker.Clear();

        var command = new CheckSensorStateCommand();

        // Act
        Result result = await Sender.Send(command);

        // Assert
        result.IsSuccess.Should().BeTrue();

        Sensor? updatedDelayedSensor = await DbContext.Sensors
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.Id == delayedSensor.Id);

        updatedDelayedSensor.Should().NotBeNull();
        updatedDelayedSensor!.State.Should().Be(SensorState.NoData);
        updatedDelayedSensor.IsDataDelayed.Should().BeTrue();

        Sensor? updatedActiveSensor = await DbContext.Sensors
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.Id == activeSensor.Id);

        updatedActiveSensor.Should().NotBeNull();
        updatedActiveSensor!.State.Should().Be(SensorState.Active);
        updatedActiveSensor.IsDataDelayed.Should().BeFalse();

        List<OutboxMessage> outboxMessages = await DbContext.OutboxMessages.AsNoTracking().ToListAsync();
        outboxMessages.Should().ContainSingle(m => m.Type.Contains("SensorNoDataDomainEvent"));
    }
}
