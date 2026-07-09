using Contracts.Events.TelemetryEvents;
using Contracts.Results;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Telemetry.Application.Features.Telemetry.Commands.AddTelemetryBatch;
using Telemetry.Domain.Entities;
using Telemetry.Infrastructure.IntegrationTests.Infrastructure;
using Telemetry.Infrastructure.Persistence.Outbox;
using Telemetry.TestShared.Builders;

namespace Telemetry.Infrastructure.IntegrationTests.Features;

public class AddTelemetryBatchHandlerTests(IntegrationTestWebAppFactory factory)
    : BaseIntegrationTest(factory)
{
    [Fact]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "<Pending>")]
    public async Task Handle_WithValidBatch_SavesTelemetryAndUpdatesSensorLastValue()
    {
        // Arrange
        Ecosystem ecosystem = new EcosystemBuilder().Build();
        Sensor sensor = new SensorBuilder()
            .WithEcosystemId(ecosystem.Id)
            .WithControllerId(ecosystem.ControllerId)
            .WithLastValue(10.0)
            .Build();

        DbContext.Ecosystems.Add(ecosystem);
        DbContext.Sensors.Add(sensor);
        await DbContext.SaveChangesAsync();
        DbContext.ChangeTracker.Clear();

        var command = new AddTelemetryBatchCommand
        {
            ControllerId = ecosystem.ControllerId,
            Items = new List<TelemetryBatchEventItem>
            {
                new()
                {
                    SensorId = sensor.Id,
                    Value = 24.5,
                    ExternalMessageId = "msg_batch_001",
                    RecordedAt = DateTime.UtcNow
                }
            }
        };

        // Act
        Result result = await Sender.Send(command);

        // Assert
        result.IsSuccess.Should().BeTrue();

        RawTelemetry? savedTelemetry = await DbContext.TelemetryRawData
            .AsNoTracking()
            .FirstOrDefaultAsync(t => t.ExternalMessageId == "msg_batch_001");

        savedTelemetry.Should().NotBeNull();
        savedTelemetry!.SensorId.Should().Be(sensor.Id);
        savedTelemetry.Value.Should().Be(24.5);

        Sensor? updatedSensor = await DbContext.Sensors
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.Id == sensor.Id);

        updatedSensor.Should().NotBeNull();
        updatedSensor!.LastValue.Should().Be(24.5);

        List<OutboxMessage> outboxMessages = await DbContext.OutboxMessages.AsNoTracking().ToListAsync();
        outboxMessages.Should().BeEmpty();
    }
}
