using Contracts.Results;
using FluentAssertions;
using Telemetry.Application.DTOs;
using Telemetry.Application.Features.Telemetry.Queries.GetRawTelemetryChart;
using Telemetry.Domain.Entities;
using Telemetry.Infrastructure.IntegrationTests.Infrastructure;
using Telemetry.TestShared.Builders;

namespace Telemetry.Infrastructure.IntegrationTests.Features;

public class GetRawTelemetryChartHandlerTests(IntegrationTestWebAppFactory factory)
    : BaseIntegrationTest(factory)
{
    [Fact]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "<Pending>")]
    public async Task Handle_WhenSensorAndRawTelemetryExist_ReturnsChartDataWithPagination()
    {
        // Arrange
        Ecosystem ecosystem = new EcosystemBuilder().Build();
        Sensor sensor = new SensorBuilder()
            .WithEcosystemId(ecosystem.Id)
            .WithName("Raw Temp Sensor")
            .WithUnit("°C")
            .Build();

        DbContext.Ecosystems.Add(ecosystem);
        DbContext.Sensors.Add(sensor);
        await DbContext.SaveChangesAsync();

        DateTime baseTime = DateTime.UtcNow.AddHours(-10);

        RawTelemetry point1 = new RawTelemetryBuilder()
            .WithSensorId(sensor.Id)
            .WithValue(18.5)
            .WithExternalMessageId("raw_p1")
            .WithRecordedAt(baseTime)
            .Build();

        RawTelemetry point2 = new RawTelemetryBuilder()
            .WithSensorId(sensor.Id)
            .WithValue(22.0)
            .WithExternalMessageId("raw_p2")
            .WithRecordedAt(baseTime.AddHours(1))
            .Build();

        RawTelemetry point3 = new RawTelemetryBuilder()
            .WithSensorId(sensor.Id)
            .WithValue(25.3)
            .WithExternalMessageId("raw_p3")
            .WithRecordedAt(baseTime.AddHours(2))
            .Build();

        DbContext.TelemetryRawData.AddRange(point1, point2, point3);
        await DbContext.SaveChangesAsync();
        DbContext.ChangeTracker.Clear();

        var query = new GetRawTelemetryChartQuery
        {
            SensorId = sensor.Id,
            From = baseTime.AddMinutes(-5),
            To = baseTime.AddHours(3),
            Skip = 1,
            Take = 1
        };

        // Act
        Result<TelemetryRawChartResponseDto> result = await Sender.Send(query);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.SensorId.Should().Be(sensor.Id);
        result.Value.SensorName.Should().Be(sensor.Name.Value);
        result.Value.Unit.Should().Be(sensor.Unit);

        result.Value.Points.Should().ContainSingle();
        TelemetryRawChartPointDto returnedPoint = result.Value.Points[0];
        returnedPoint.SensorId.Should().Be(sensor.Id);
        returnedPoint.Value.Should().Be(22.0);
        returnedPoint.RecordedAt.Should().BeCloseTo(baseTime.AddHours(1), TimeSpan.FromSeconds(1));
    }
}
