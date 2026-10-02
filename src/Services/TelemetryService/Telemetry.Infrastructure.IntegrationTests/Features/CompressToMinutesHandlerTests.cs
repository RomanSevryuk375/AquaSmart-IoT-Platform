using BuildingBlocks.Domain.Enums;
using BuildingBlocks.Domain.Results;
using BuildingBlocks.Infrastructure.Data.Outbox;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Telemetry.Application.Features.BackgroundJobs.Commands.CompressToMinutes;
using Telemetry.Domain.Entities;
using Telemetry.Domain.Events;
using Telemetry.Infrastructure.IntegrationTests.Infrastructure;
using Telemetry.TestShared.Builders;

namespace Telemetry.Infrastructure.IntegrationTests.Features;

public class CompressToMinutesHandlerTests(IntegrationTestWebAppFactory factory)
    : BaseIntegrationTest(factory)
{
    [Fact]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "<Pending>")]
    public async Task Handle_WhenRawDataExists_AggregatesAndSavesToDatabase()
    {
        // Arrange
        Ecosystem ecosystem = new EcosystemBuilder().Build();
        Sensor sensor = new SensorBuilder()
            .WithEcosystemId(ecosystem.Id)
            .Build();

        DbContext.Ecosystems.Add(ecosystem);
        DbContext.Sensors.Add(sensor);
        await DbContext.SaveChangesAsync();

        var to = new DateTime(
            DateTime.UtcNow.Year, DateTime.UtcNow.Month, DateTime.UtcNow.Day,
            DateTime.UtcNow.Hour, DateTime.UtcNow.Minute, 0, DateTimeKind.Utc);
        DateTime from = to.AddMinutes(-1);

        RawTelemetry raw1 = new RawTelemetryBuilder()
            .WithSensorId(sensor.Id)
            .WithEcosystemId(ecosystem.Id)
            .WithValue(20.0)
            .WithExternalMessageId("raw_msg_1")
            .WithRecordedAt(from.AddSeconds(10))
            .Build();

        RawTelemetry raw2 = new RawTelemetryBuilder()
            .WithSensorId(sensor.Id)
            .WithEcosystemId(ecosystem.Id)
            .WithValue(30.0)
            .WithExternalMessageId("raw_msg_2")
            .WithRecordedAt(from.AddSeconds(40))
            .Build();

        RawTelemetry outsideRaw = new RawTelemetryBuilder()
            .WithSensorId(sensor.Id)
            .WithEcosystemId(ecosystem.Id)
            .WithValue(50.0)
            .WithExternalMessageId("raw_msg_outside")
            .WithRecordedAt(from.AddMinutes(-1).AddSeconds(20))
            .Build();

        RawTelemetry activeRaw = new RawTelemetryBuilder()
            .WithSensorId(sensor.Id)
            .WithEcosystemId(ecosystem.Id)
            .WithValue(40.0)
            .WithExternalMessageId("raw_msg_active_minute")
            .WithRecordedAt(to.AddSeconds(15))
            .Build();

        DbContext.TelemetryRawData.AddRange(raw1, raw2, outsideRaw, activeRaw);
        await DbContext.SaveChangesAsync();
        DbContext.ChangeTracker.Clear();

        var command = new CompressToMinutesCommand();

        // Act
        Result result = await Sender.Send(command);

        // Assert
        result.IsSuccess.Should().BeTrue();

        // Check aggregate for current window [from, to)
        AggregateTelemetry? aggregate = await DbContext.TelemetryAggregateData
            .AsNoTracking()
            .FirstOrDefaultAsync(a => a.SensorId == sensor.Id && a.Period == PeriodType.Minute && a.PeriodStart == from);

        aggregate.Should().NotBeNull();
        aggregate!.Summary.MinValue.Should().Be(20.0);
        aggregate.Summary.MaxValue.Should().Be(30.0);
        aggregate.Summary.AvgValue.Should().Be(25.0);
        aggregate.Summary.Count.Should().Be(2);

        // Check aggregate for buffered offline window [from - 1 min, from)
        DateTime outsideFrom = from.AddMinutes(-1);
        AggregateTelemetry? outsideAggregate = await DbContext.TelemetryAggregateData
            .AsNoTracking()
            .FirstOrDefaultAsync(a => a.SensorId == sensor.Id && a.Period == PeriodType.Minute && a.PeriodStart == outsideFrom);

        outsideAggregate.Should().NotBeNull();
        outsideAggregate!.Summary.MinValue.Should().Be(50.0);
        outsideAggregate.Summary.MaxValue.Should().Be(50.0);
        outsideAggregate.Summary.AvgValue.Should().Be(50.0);
        outsideAggregate.Summary.Count.Should().Be(1);

        // Both completed minutes (including buffered offline) should now be marked as aggregated
        List<RawTelemetry> completedRawData = await DbContext.TelemetryRawData
            .AsNoTracking()
            .Where(r => r.SensorId == sensor.Id && r.ExternalMessageId != "raw_msg_active_minute")
            .ToListAsync();

        completedRawData.Should().HaveCount(3);
        completedRawData.Should().AllSatisfy(r => r.IsAggregated.Should().BeTrue());

        // The active minute raw telemetry must NOT be aggregated yet
        RawTelemetry? activeMinuteRaw = await DbContext.TelemetryRawData
            .AsNoTracking()
            .FirstOrDefaultAsync(r => r.ExternalMessageId == "raw_msg_active_minute");

        activeMinuteRaw.Should().NotBeNull();
        activeMinuteRaw!.IsAggregated.Should().BeFalse();

        List<OutboxMessage> outboxMessages = await DbContext.OutboxMessages.AsNoTracking().ToListAsync();
        outboxMessages.Should().HaveCount(2);
        outboxMessages.Should().AllSatisfy(m => m.Type.Should().Contain(nameof(AggregatedTelemetryAddedDomainEvent)));
    }
}
