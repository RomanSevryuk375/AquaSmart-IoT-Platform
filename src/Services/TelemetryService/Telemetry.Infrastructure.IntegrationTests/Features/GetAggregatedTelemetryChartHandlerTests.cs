using Contracts.Enums;
using Contracts.Results;
using FluentAssertions;
using Telemetry.Application.DTOs;
using Telemetry.Application.Features.Telemetry.Queries.GetAggregatedTelemetryChart;
using Telemetry.Domain.Entities;
using Telemetry.Infrastructure.IntegrationTests.Infrastructure;
using Telemetry.TestShared.Builders;

namespace Telemetry.Infrastructure.IntegrationTests.Features;

public class GetAggregatedTelemetryChartHandlerTests(IntegrationTestWebAppFactory factory)
    : BaseIntegrationTest(factory)
{
    [Fact]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "<Pending>")]
    public async Task Handle_WhenSensorAndAggregatedDataExist_ReturnsChartDataWithPagination()
    {
        // Arrange
        Ecosystem ecosystem = new EcosystemBuilder().Build();
        Sensor sensor = new SensorBuilder()
            .WithEcosystemId(ecosystem.Id)
            .WithName("Agg Temp Sensor")
            .WithUnit("°C")
            .Build();

        DbContext.Ecosystems.Add(ecosystem);
        DbContext.Sensors.Add(sensor);
        await DbContext.SaveChangesAsync();

        DateTime baseTime = DateTime.UtcNow.Date.AddHours(-5);

        AggregateTelemetry agg1 = new AggregateTelemetryBuilder()
            .WithSensorId(sensor.Id)
            .WithPeriod(PeriodType.Hourly)
            .WithPeriodStart(baseTime)
            .WithValues(minValue: 10.0, maxValue: 20.0, avgValue: 15.0, dataPointsCount: 5)
            .Build();

        AggregateTelemetry agg2 = new AggregateTelemetryBuilder()
            .WithSensorId(sensor.Id)
            .WithPeriod(PeriodType.Hourly)
            .WithPeriodStart(baseTime.AddHours(1))
            .WithValues(minValue: 12.0, maxValue: 22.0, avgValue: 17.0, dataPointsCount: 6)
            .Build();

        AggregateTelemetry agg3 = new AggregateTelemetryBuilder()
            .WithSensorId(sensor.Id)
            .WithPeriod(PeriodType.Hourly)
            .WithPeriodStart(baseTime.AddHours(2))
            .WithValues(minValue: 14.0, maxValue: 24.0, avgValue: 19.0, dataPointsCount: 7)
            .Build();

        DbContext.TelemetryAggregateData.AddRange(agg1, agg2, agg3);
        await DbContext.SaveChangesAsync();
        DbContext.ChangeTracker.Clear();

        var query = new GetAggregatedTelemetryChartQuery
        {
            SensorId = sensor.Id,
            Period = PeriodType.Hourly,
            From = baseTime.AddMinutes(-30),
            To = baseTime.AddHours(3),
            Skip = 1,
            Take = 1
        };

        // Act
        Result<TelemetryChartResponseDto> result = await Sender.Send(query);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.SensorId.Should().Be(sensor.Id);
        result.Value.SensorName.Should().Be(sensor.Name.Value);
        result.Value.Unit.Should().Be(sensor.Unit);

        result.Value.Points.Should().ContainSingle();
        TelemetryChartPointDto returnedPoint = result.Value.Points[0];
        returnedPoint.SensorId.Should().Be(sensor.Id);
        returnedPoint.MinValue.Should().Be(12.0);
        returnedPoint.MaxValue.Should().Be(22.0);
        returnedPoint.AvgValue.Should().Be(17.0);
        returnedPoint.Time.Should().BeCloseTo(baseTime.AddHours(1), TimeSpan.FromSeconds(1));
    }
}
