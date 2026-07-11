using Contracts.Enums;
using Contracts.Results;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Telemetry.Application.Features.BackgroundJobs.Commands.CompressToMinutes;
using Telemetry.Domain.Entities;
using Telemetry.Domain.Events;
using Telemetry.Infrastructure.IntegrationTests.Infrastructure;
using Telemetry.Infrastructure.Persistence.Outbox;
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
            .WithRecordedAt(from.AddMinutes(-1))
            .Build();

        DbContext.TelemetryRawData.AddRange(raw1, raw2, outsideRaw);
        await DbContext.SaveChangesAsync();
        DbContext.ChangeTracker.Clear();

        var command = new CompressToMinutesCommand();

        // Act
        Result result = await Sender.Send(command);

        // Assert
        result.IsSuccess.Should().BeTrue();

        AggregateTelemetry? aggregate = await DbContext.TelemetryAggregateData
            .AsNoTracking()
            .FirstOrDefaultAsync(a => a.SensorId == sensor.Id && a.Period == PeriodType.Minute && a.PeriodStart == from);

        aggregate.Should().NotBeNull();
        aggregate!.Summary.MinValue.Should().Be(20.0);
        aggregate.Summary.MaxValue.Should().Be(30.0);
        aggregate.Summary.AvgValue.Should().Be(25.0);
        aggregate.Summary.Count.Should().Be(2);

        List<RawTelemetry> processedRawData = await DbContext.TelemetryRawData
            .AsNoTracking()
            .Where(r => r.SensorId == sensor.Id && r.ExternalMessageId != "raw_msg_outside")
            .ToListAsync();

        processedRawData.Should().HaveCount(2);
        processedRawData.Should().AllSatisfy(r => r.IsAggregated.Should().BeTrue());

        RawTelemetry? unprocessedRaw = await DbContext.TelemetryRawData
            .AsNoTracking()
            .FirstOrDefaultAsync(r => r.ExternalMessageId == "raw_msg_outside");

        unprocessedRaw.Should().NotBeNull();
        unprocessedRaw!.IsAggregated.Should().BeFalse();

        List<OutboxMessage> outboxMessages = await DbContext.OutboxMessages.AsNoTracking().ToListAsync();
        outboxMessages.Should().ContainSingle();
        OutboxMessage outboxMessage = outboxMessages.Single();
        outboxMessage.Type.Should().Contain(nameof(AggregatedTelemetryAddedDomainEvent));

        AggregatedTelemetryAddedDomainEvent? deserializedEvent = JsonConvert
            .DeserializeObject<AggregatedTelemetryAddedDomainEvent>(
            outboxMessage.Content,
            new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.All });

        deserializedEvent.Should().NotBeNull();
        deserializedEvent!.SensorId.Should().Be(sensor.Id);
        deserializedEvent.EcosystemId.Should().Be(ecosystem.Id);
        deserializedEvent.Period.Should().Be(PeriodType.Minute);
        deserializedEvent.MinValue.Should().Be(20.0);
        deserializedEvent.MaxValue.Should().Be(30.0);
        deserializedEvent.AvgValue.Should().Be(25.0);
    }
}
