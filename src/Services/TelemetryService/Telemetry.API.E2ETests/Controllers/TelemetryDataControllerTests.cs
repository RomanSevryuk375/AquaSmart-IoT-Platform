using System.Net;
using System.Net.Http.Json;
using Contracts.Enums;
using Contracts.Events.TelemetryEvents;
using Contracts.Results;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using Telemetry.API.E2ETests.Infrastructure;
using Telemetry.Application.DTOs;
using Telemetry.Application.Interfaces;
using Telemetry.Domain.Entities;
using Telemetry.TestShared.Builders;
using Telemetry.TestShared.Constants;

namespace Telemetry.API.E2ETests.Controllers;

public class TelemetryDataControllerTests(E2ETestWebAppFactory factory) : BaseE2ETest(factory)
{
    [Fact]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "<Pending>")]
    public async Task GetAllRawDataAsync_WithoutAuthorizationHeader_Returns401Unauthorized()
    {
        // Arrange
        Client.DefaultRequestHeaders.Authorization = null;

        // Act
        HttpResponseMessage response = await Client.GetAsync(
            "api/telemetry/v1/data/raw?sensorId=" + Guid.NewGuid());

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "<Pending>")]
    public async Task GetAllRawDataAsync_WithValidRequest_Returns200OKAndCorrectData()
    {
        // Arrange
        Ecosystem ecosystem = new EcosystemBuilder().Build();
        Sensor sensor = new SensorBuilder()
            .WithEcosystemId(ecosystem.Id)
            .WithName("Raw Temp Sensor")
            .WithUnit("°C")
            .Build();

        DateTime baseTime = DateTime.UtcNow.AddMinutes(-10);

        RawTelemetry telemetry1 = new RawTelemetryBuilder()
            .WithSensorId(sensor.Id)
            .WithEcosystemId(ecosystem.Id)
            .WithValue(25.5)
            .WithExternalMessageId("raw_p1")
            .WithRecordedAt(baseTime.AddMinutes(5))
            .Build();

        RawTelemetry telemetry2 = new RawTelemetryBuilder()
            .WithSensorId(sensor.Id)
            .WithEcosystemId(ecosystem.Id)
            .WithValue(26.0)
            .WithExternalMessageId("raw_p2")
            .WithRecordedAt(baseTime)
            .Build();

        DbContext.Ecosystems.Add(ecosystem);
        DbContext.Sensors.Add(sensor);
        DbContext.TelemetryRawData.AddRange(telemetry1, telemetry2);
        await DbContext.SaveChangesAsync();
        DbContext.ChangeTracker.Clear();

        // Act
        HttpResponseMessage response = await Client.GetAsync(
            $"api/telemetry/v1/data/raw?sensorId={sensor.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        TelemetryRawChartResponseDto? content = await response.Content
            .ReadFromJsonAsync<TelemetryRawChartResponseDto>();
        content.Should().NotBeNull();
        content!.SensorId.Should().Be(sensor.Id);
        content.SensorName.Should().Be(sensor.Name.Value);
        content.Unit.Should().Be(sensor.Unit);

        content.Points.Should().HaveCount(2);

        content.Points[0].RecordedAt.Should().BeBefore(content.Points[1].RecordedAt);
        content.Points[0].Value.Should().Be(26.0);
        content.Points[1].Value.Should().Be(25.5);
    }

    [Fact]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "<Pending>")]
    public async Task GetAllRawDataAsync_WhenSensorDoesNotExist_Returns404NotFound()
    {
        // Arrange
        var nonExistentSensorId = Guid.NewGuid();

        // Act
        HttpResponseMessage response = await Client.GetAsync(
            $"api/telemetry/v1/data/raw?sensorId={nonExistentSensorId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "<Pending>")]
    public async Task GetAllAggregatedDataAsync_WithoutAuthorizationHeader_Returns401Unauthorized()
    {
        // Arrange
        Client.DefaultRequestHeaders.Authorization = null;

        // Act
        HttpResponseMessage response = await Client.GetAsync(
            $"api/telemetry/v1/data/aggregate?sensorId={Guid.NewGuid()}&period={PeriodType.Hourly}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "<Pending>")]
    public async Task GetAllAggregatedDataAsync_WithValidRequest_Returns200OKAndCorrectData()
    {
        // Arrange
        Ecosystem ecosystem = new EcosystemBuilder().Build();
        Sensor sensor = new SensorBuilder()
            .WithEcosystemId(ecosystem.Id)
            .WithName("Agg Temp Sensor")
            .WithUnit("°C")
            .Build();

        DateTime baseTime = DateTime.UtcNow.AddHours(-5);

        AggregateTelemetry agg1 = new AggregateTelemetryBuilder()
            .WithSensorId(sensor.Id)
            .WithEcosystemId(ecosystem.Id)
            .WithPeriod(PeriodType.Hourly)
            .WithPeriodStart(baseTime.AddHours(2))
            .WithValues(minValue: 10.0, maxValue: 20.0, avgValue: 15.0, dataPointsCount: 5)
            .Build();

        AggregateTelemetry agg2 = new AggregateTelemetryBuilder()
            .WithSensorId(sensor.Id)
            .WithEcosystemId(ecosystem.Id)
            .WithPeriod(PeriodType.Hourly)
            .WithPeriodStart(baseTime)
            .WithValues(minValue: 12.0, maxValue: 22.0, avgValue: 17.0, dataPointsCount: 6)
            .Build();

        DbContext.Ecosystems.Add(ecosystem);
        DbContext.Sensors.Add(sensor);
        DbContext.TelemetryAggregateData.AddRange(agg1, agg2);
        await DbContext.SaveChangesAsync();
        DbContext.ChangeTracker.Clear();

        // Act
        HttpResponseMessage response = await Client.GetAsync(
            $"api/telemetry/v1/data/aggregate?sensorId={sensor.Id}&period={PeriodType.Hourly}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        TelemetryChartResponseDto? content = await response.Content.ReadFromJsonAsync<TelemetryChartResponseDto>();
        content.Should().NotBeNull();
        content!.SensorId.Should().Be(sensor.Id);
        content.SensorName.Should().Be(sensor.Name.Value);
        content.Unit.Should().Be(sensor.Unit);

        content.Points.Should().HaveCount(2);

        content.Points[0].Time.Should().BeBefore(content.Points[1].Time);
        content.Points[0].AvgValue.Should().Be(17.0);
        content.Points[1].AvgValue.Should().Be(15.0);
    }

    [Fact]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "<Pending>")]
    public async Task GetAllAggregatedDataAsync_WhenSensorDoesNotExist_Returns404NotFound()
    {
        // Arrange
        var nonExistentSensorId = Guid.NewGuid();

        // Act
        HttpResponseMessage response = await Client.GetAsync(
            $"api/telemetry/v1/data/aggregate?sensorId={nonExistentSensorId}&period={PeriodType.Hourly}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "<Pending>")]
    public async Task ReceiveBatchTelemetry_WithValidToken_Returns202Accepted()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var controllerId = Guid.NewGuid();

        Ecosystem ecosystem = new EcosystemBuilder()
            .WithUserId(userId)
            .WithControllerId(controllerId)
            .Build();

        Sensor sensor = new SensorBuilder()
            .WithEcosystemId(ecosystem.Id)
            .WithControllerId(controllerId)
            .Build();

        DbContext.Ecosystems.Add(ecosystem);
        DbContext.Sensors.Add(sensor);
        await DbContext.SaveChangesAsync();
        DbContext.ChangeTracker.Clear();

        IDeviceTokenValidator deviceTokenValidator = Factory.Services.GetRequiredService<IDeviceTokenValidator>();
        deviceTokenValidator.ValidateAsync(
                TestConstants.ValidMacAddress,
                TestConstants.ValidDeviceToken,
                Arg.Any<CancellationToken>())
            .Returns(Result<ValidateResponseDto>.Success(new ValidateResponseDto
            {
                ControllerId = controllerId,
                UserId = userId
            }));

        var payload = new AddTelemetryBatchRequestDto
        {
            MacAddress = TestConstants.ValidMacAddress,
            Items = new List<TelemetryBatchEventItem>
            {
                new()
                {
                    SensorId = sensor.Id,
                    Value = 24.5,
                    ExternalMessageId = "msg-123",
                    RecordedAt = DateTime.UtcNow
                }
            }
        };

        Client.DefaultRequestHeaders.Add("X-Device-Token", TestConstants.ValidDeviceToken);

        // Act
        HttpResponseMessage response = await Client.PostAsJsonAsync("api/telemetry/v1/data", payload);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Accepted);

        RawTelemetry? savedTelemetry = await DbContext.TelemetryRawData
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.SensorId == sensor.Id);

        savedTelemetry.Should().NotBeNull();
        savedTelemetry!.Value.Should().Be(24.5);
    }

    [Fact]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "<Pending>")]
    public async Task ReceiveBatchTelemetry_WithInvalidToken_Returns409Conflict()
    {
        // Arrange
        IDeviceTokenValidator deviceTokenValidator = Factory.Services.GetRequiredService<IDeviceTokenValidator>();
        const string badToken = "invalid_token";

        deviceTokenValidator.ValidateAsync(
                Arg.Any<string>(),
                badToken,
                Arg.Any<CancellationToken>())
            .Returns(Result<ValidateResponseDto>.Failure(Error.Conflict("Device.InvalidToken", "The token is invalid.")));

        var payload = new AddTelemetryBatchRequestDto
        {
            MacAddress = TestConstants.ValidMacAddress,
            Items = new List<TelemetryBatchEventItem>
            {
                new()
                {
                    SensorId = Guid.NewGuid(),
                    Value = 12.3,
                    ExternalMessageId = "msg-456",
                    RecordedAt = DateTime.UtcNow
                }
            }
        };

        Client.DefaultRequestHeaders.Add("X-Device-Token", badToken);

        // Act
        HttpResponseMessage response = await Client.PostAsJsonAsync("api/telemetry/v1/data", payload);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }
}
