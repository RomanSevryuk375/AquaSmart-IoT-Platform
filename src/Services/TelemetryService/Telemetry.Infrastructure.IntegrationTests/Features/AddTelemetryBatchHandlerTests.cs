using BuildingBlocks.Domain.Results;
using BuildingBlocks.Infrastructure.Data.Outbox;
using BuildingBlocks.IntegrationEvents.Events.Telemetrys;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Telemetry.Application.Features.Telemetry.Commands.AddTelemetryBatch;
using Telemetry.Application.Features.Telemetry.Commands.IngestDeviceTelemetryBatch;
using Telemetry.Domain.Entities;
using Telemetry.Domain.Events;
using Telemetry.Domain.Interfaces;
using Telemetry.Infrastructure.IntegrationTests.Infrastructure;
using Telemetry.TestShared.Builders;
using Telemetry.TestShared.Constants;

namespace Telemetry.Infrastructure.IntegrationTests.Features;

public class AddTelemetryBatchHandlerTests(IntegrationTestWebAppFactory factory)
    : BaseIntegrationTest(factory)
{
    [Fact]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "<Pending>")]
    public async Task Handle_WithValidBatch_SavesTelemetryAndUpdatesSensorLastValue()
    {
        // Arrange
        Ecosystem ecosystem = new EcosystemBuilder()
            .WithControllerId(TestConstants.ControllerId)
            .WithUserId(TestConstants.UserId)
            .Build();
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
            MacAddress = TestConstants.ValidMacAddress,
            DeviceToken = TestConstants.ValidDeviceToken,
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
        outboxMessages.Should().ContainSingle();
        OutboxMessage outboxMessage = outboxMessages.Single();
        outboxMessage.Type.Should().Contain(nameof(RawTelemetryAddedDomainEvent));

        RawTelemetryAddedDomainEvent? deserializedEvent = JsonConvert.DeserializeObject<RawTelemetryAddedDomainEvent>(
            outboxMessage.Content,
            new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.All });

        deserializedEvent.Should().NotBeNull();
        deserializedEvent!.SensorId.Should().Be(sensor.Id);
        deserializedEvent.EcosystemId.Should().Be(ecosystem.Id);
        deserializedEvent.Value.Should().Be(24.5);
    }

    [Fact]
    public async Task Handle_FromDeviceTelemetryEvent_SavesTelemetryAndOutboxMessage()
    {
        // Arrange
        Ecosystem ecosystem = new EcosystemBuilder()
            .WithControllerId(TestConstants.ControllerId)
            .WithUserId(TestConstants.UserId)
            .Build();
        Sensor sensor = new SensorBuilder()
            .WithEcosystemId(ecosystem.Id)
            .WithControllerId(ecosystem.ControllerId)
            .Build();

        DbContext.Ecosystems.Add(ecosystem);
        DbContext.Sensors.Add(sensor);
        await DbContext.SaveChangesAsync();
        DbContext.ChangeTracker.Clear();

        var command = new IngestDeviceTelemetryBatchCommand
        {
            ControllerId = ecosystem.ControllerId,
            UserId = ecosystem.UserId,
            MacAddress = TestConstants.ValidMacAddress,
            Items =
            [
                new TelemetryBatchEventItem
                {
                    SensorId = sensor.Id,
                    Value = 27.3,
                    ExternalMessageId = "mqtt_batch_001",
                    RecordedAt = DateTime.UtcNow
                }
            ]
        };

        // Act
        Result result = await Sender.Send(command);

        // Assert
        result.IsSuccess.Should().BeTrue();
        RawTelemetry? saved = await DbContext.TelemetryRawData
            .AsNoTracking()
            .FirstOrDefaultAsync(t => t.ExternalMessageId == "mqtt_batch_001");
        saved.Should().NotBeNull();
        saved!.Value.Should().Be(27.3);

        List<OutboxMessage> outbox = await DbContext.OutboxMessages.AsNoTracking().ToListAsync();
        outbox.Should().ContainSingle(m => m.Type.Contains(nameof(RawTelemetryAddedDomainEvent)));
    }

    [Fact]
    public async Task Handle_WithDuplicateExternalMessageIdsInSameBatchAndAcrossBatches_DoesNotDuplicateOrThrow()
    {
        // Arrange
        Ecosystem ecosystem = new EcosystemBuilder()
            .WithControllerId(TestConstants.ControllerId)
            .WithUserId(TestConstants.UserId)
            .Build();
        Sensor sensor = new SensorBuilder()
            .WithEcosystemId(ecosystem.Id)
            .WithControllerId(ecosystem.ControllerId)
            .Build();

        DbContext.Ecosystems.Add(ecosystem);
        DbContext.Sensors.Add(sensor);
        await DbContext.SaveChangesAsync();
        DbContext.ChangeTracker.Clear();

        var command1 = new IngestDeviceTelemetryBatchCommand
        {
            ControllerId = ecosystem.ControllerId,
            UserId = ecosystem.UserId,
            MacAddress = TestConstants.ValidMacAddress,
            Items =
            [
                new TelemetryBatchEventItem
                {
                    SensorId = sensor.Id,
                    Value = 21.0,
                    ExternalMessageId = "dup_batch_msg",
                    RecordedAt = DateTime.UtcNow
                },
                new TelemetryBatchEventItem
                {
                    SensorId = sensor.Id,
                    Value = 22.0,
                    ExternalMessageId = "dup_batch_msg",
                    RecordedAt = DateTime.UtcNow
                }
            ]
        };

        // Act
        Result firstResult = await Sender.Send(command1);
        Result secondResult = await Sender.Send(command1);

        // Assert
        firstResult.IsSuccess.Should().BeTrue();
        secondResult.IsSuccess.Should().BeTrue();

        List<RawTelemetry> rows = await DbContext.TelemetryRawData
            .AsNoTracking()
            .Where(x => x.ExternalMessageId == "dup_batch_msg")
            .ToListAsync();
        rows.Should().ContainSingle();
    }

    [Fact]
    public async Task SaveBatchWithDuplicateHandlingAsync_WhenUniqueViolation23505Occurs_SavesNonConflictingAndIgnoresDuplicate()
    {
        // Arrange
        Ecosystem ecosystem = new EcosystemBuilder()
            .WithControllerId(TestConstants.ControllerId)
            .WithUserId(TestConstants.UserId)
            .Build();
        Sensor sensor = new SensorBuilder()
            .WithEcosystemId(ecosystem.Id)
            .WithControllerId(ecosystem.ControllerId)
            .Build();
        RawTelemetry existing = new RawTelemetryBuilder()
            .WithSensorId(sensor.Id)
            .WithEcosystemId(ecosystem.Id)
            .WithExternalMessageId("race_msg_exists")
            .Build();

        DbContext.Ecosystems.Add(ecosystem);
        DbContext.Sensors.Add(sensor);
        DbContext.TelemetryRawData.Add(existing);
        await DbContext.SaveChangesAsync();
        DbContext.ChangeTracker.Clear();

        await using var tx = await DbContext.Database.BeginTransactionAsync();
        ITelemetryRawDataRepository repo = GetRequiredService<ITelemetryRawDataRepository>();

        RawTelemetry conflicting = RawTelemetry.Create(
            Guid.NewGuid(), sensor.Id, ecosystem.Id, 20.0, "race_msg_exists", DateTime.UtcNow).Value;
        RawTelemetry fresh = RawTelemetry.Create(
            Guid.NewGuid(), sensor.Id, ecosystem.Id, 25.0, "race_msg_fresh", DateTime.UtcNow).Value;

        await repo.AddAsync(conflicting);
        await repo.AddAsync(fresh);

        // Act
        await repo.SaveBatchWithDuplicateHandlingAsync();
        await tx.CommitAsync();
        DbContext.ChangeTracker.Clear();

        // Assert
        List<RawTelemetry> saved = await DbContext.TelemetryRawData.AsNoTracking().ToListAsync();
        saved.Should().HaveCount(2);
        saved.Should().ContainSingle(x => x.ExternalMessageId == "race_msg_exists");
        saved.Should().ContainSingle(x => x.ExternalMessageId == "race_msg_fresh");
    }
}
