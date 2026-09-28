using BuildingBlocks.Domain.Enums;
using BuildingBlocks.Domain.Results;
using Device.Application.Features.Controllers.Query.GetControllerConfig;
using Device.Domain.Entities.Sensors;

namespace Device.Infrastructure.IntegrationTests.Features.Controllers;

public class GetControllerConfigHandlerTests(
    IntegrationTestWebAppFactory factory) : BaseIntegrationTest(factory)
{
    [Fact]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "<Pending>")]
    public async Task GetConfig_WithValidMacAddress_ReturnsControllerWithSensorsAndRelays()
    {
        // Arrange
        Controller controller = new ControllerBuilder().Build();

        Sensor sensor1 = new SensorBuilder()
            .WithId(Guid.NewGuid())
            .WithControllerId(controller.Id)
            .WithType(SensorType.Temperature)
            .Build();

        Sensor sensor2 = new SensorBuilder()
            .WithId(Guid.NewGuid())
            .WithControllerId(controller.Id)
            .WithType(SensorType.Humidity)
            .WithAddress(ConnectionProtocol.OneWire, "28FF4A1B2C3D4E5F")
            .Build();

        Relay relay = new RelayBuilder()
            .WithControllerId(controller.Id)
            .Build();

        DbContext.Controllers.Add(controller);
        DbContext.Sensors.AddRange(sensor1, sensor2);
        DbContext.Relays.Add(relay);
        await DbContext.SaveChangesAsync();

        var query = new GetControllerConfigQuery
        {
            MacAddress = controller.MacAddress.Value
        };

        // Act
        Result<ControllerConfig> result = await Sender.Send(query);

        // Assert
        result.IsSuccess.Should().BeTrue();

        ControllerConfig config = result.Value;
        config.SendIntervalMs.Should().BeGreaterThan(0);
        config.Sensors.Should().HaveCount(2);
        config.Relays.Should().HaveCount(1);
    }

    [Fact]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "<Pending>")]
    public async Task GetConfig_WhenNoRelaysOrSensors_ReturnsEmptyCollections()
    {
        // Arrange
        Controller controller = new ControllerBuilder().Build();

        DbContext.Controllers.Add(controller);
        await DbContext.SaveChangesAsync();

        var query = new GetControllerConfigQuery
        {
            MacAddress = controller.MacAddress.Value
        };

        // Act
        Result<ControllerConfig> result = await Sender.Send(query);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Sensors.Should().BeEmpty();
        result.Value.Relays.Should().BeEmpty();
    }
}
