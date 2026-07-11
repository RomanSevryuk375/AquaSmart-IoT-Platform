using Contracts.Enums;
using Device.Application.Extesions;
using Device.Application.Features.Sensors.Command.AddSensor;
using Device.Domain.Entities;
using Device.Domain.Entities.Sensors;

namespace Device.API.E2ETests.Controllers;

public class SensorsControllerTests(E2ETestWebAppFactory factory) : BaseE2ETest(factory)
{
    [Fact]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "<Pending>")]
    public async Task AddSensor_WithValidData_Returns201CreatedAndLocationHeader()
    {
        // Arrange
        Controller controller = new ControllerBuilder()
            .WithId(TestConstants.ControllerId)
            .WithUserId(TestConstants.UserId)
            .Build();

        DbContext.Controllers.Add(controller);
        await DbContext.SaveChangesAsync();

        var command = new AddSensorCommand
        {
            ControllerId = TestConstants.ControllerId,
            Name = "Water Temp Sensor",
            ConnectionProtocol = ConnectionProtocol.OneWire,
            ConnectionAddress = "28FF4A1B2C3D4E5F",
            Type = SensorType.Temperature,
            Unit = "°C"
        };

        // Act
        HttpResponseMessage response = await Client.PostAsJsonAsync(ApiConstants.Routes.Sensors, command);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        response.Headers.Location.Should().NotBeNull();
        response.Headers.Location!.ToString().Should().Contain($"{ApiConstants.Routes.Sensors}/");

        SensorCreatedResponse? content = await response.Content.ReadFromJsonAsync<SensorCreatedResponse>();
        content.Should().NotBeNull();
        content!.Id.Should().NotBeEmpty();
    }

    [Fact]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "<Pending>")]
    public async Task AddSensor_WithInvalidAddress_Returns400BadRequest()
    {
        // Arrange
        Controller controller = new ControllerBuilder()
            .WithId(TestConstants.ControllerId)
            .WithUserId(TestConstants.UserId)
            .Build();

        DbContext.Controllers.Add(controller);
        await DbContext.SaveChangesAsync();

        var command = new AddSensorCommand
        {
            ControllerId = TestConstants.ControllerId,
            Name = "Test",
            ConnectionProtocol = ConnectionProtocol.I2C,
            ConnectionAddress = "invalid_hex",
            Type = SensorType.Temperature,
            Unit = UnitConstants.Celsius
        };

        // Act
        HttpResponseMessage response = await Client.PostAsJsonAsync(
            ApiConstants.Routes.Sensors, command);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        ErrorResponseDummy? errorResponse = await response.Content
            .ReadFromJsonAsync<ErrorResponseDummy>();
        errorResponse!.Message.Should().NotBeNullOrEmpty();
    }

    [Fact]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "<Pending>")]
    public async Task GetSensorById_WhenBelongsToAnotherUser_Returns404NotFound()
    {
        // Arrange
        var hackerUserId = Guid.NewGuid();
        var hackerControllerId = Guid.NewGuid();

        Controller hackerController = new ControllerBuilder()
            .WithId(hackerControllerId)
            .WithUserId(hackerUserId)
            .Build();

        Sensor hackerSensor = new SensorBuilder()
            .WithId(Guid.NewGuid())
            .WithUserId(hackerUserId)
            .WithControllerId(hackerControllerId)
            .Build();

        DbContext.Controllers.Add(hackerController);
        DbContext.Sensors.Add(hackerSensor);
        await DbContext.SaveChangesAsync();

        // Act
        HttpResponseMessage response = await Client.GetAsync(
            $"{ApiConstants.Routes.Sensors}/{hackerSensor.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }


    [Fact]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "<Pending>")]
    public async Task DeleteSensor_WithValidId_Returns204NoContent()
    {
        // Arrange
        Controller controller = new ControllerBuilder()
            .WithId(TestConstants.ControllerId)
            .WithUserId(TestConstants.UserId)
            .Build();

        Sensor sensor = new SensorBuilder()
            .WithId(TestConstants.SensorId)
            .WithControllerId(TestConstants.ControllerId)
            .Build();

        DbContext.Controllers.Add(controller);
        DbContext.Sensors.Add(sensor);
        await DbContext.SaveChangesAsync();

        // Act
        HttpResponseMessage response = await Client.DeleteAsync(
            $"{ApiConstants.Routes.Sensors}/{TestConstants.SensorId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    private record ErrorResponseDummy(int StatusCode, string Message);
}
