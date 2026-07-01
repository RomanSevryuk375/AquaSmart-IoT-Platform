using Contracts.Enums;
using Contracts.Results;
using Device.Application.Extesions;
using Device.Application.Features.Controllers.Query.GetControllerConfig;
using Device.Domain.Entities;
using Device.Domain.Entities.Sensors;
using Device.Infrastructure.IntegrationTests.Infrastructure;
using Device.TestShared.Builders;
using FluentAssertions;

namespace Device.Infrastructure.IntegrationTests.Features.Controllers;

public class GetControllerConfigHandlerTests(
    IntegrationTestWebAppFactory factory) : BaseIntegrationTest(factory)
{
    [Fact]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "<Pending>")]
    public async Task GetConfig_WithValidCredentials_ReturnsControllerWithSensorsAndRelays()
    {
        // Arrange
        var hasher = new MyHasher();
        string rawToken = "my_super_secret_token";
        string realTokenHash = hasher.Generate(rawToken);

        Controller controller = new ControllerBuilder()
            .WithDeviceTokenHash(realTokenHash)
            .Build();

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
            MacAddress = controller.MacAddress.Value,
            DeviceToken = rawToken
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
    public async Task GetConfig_WithInvalidToken_ReturnsNotFound()
    {
        // Arrange
        var hasher = new MyHasher();
        string realTokenHash = hasher.Generate("my_super_secret_token");

        Controller controller = new ControllerBuilder()
            .WithDeviceTokenHash(realTokenHash)
            .Build();

        DbContext.Controllers.Add(controller);
        await DbContext.SaveChangesAsync();

        var query = new GetControllerConfigQuery
        {
            MacAddress = controller.MacAddress.Value,
            DeviceToken = "wrong_token_from_hacker"
        };

        // Act
        Result<ControllerConfig> result = await Sender.Send(query);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Controller.NotFound");
    }
}
