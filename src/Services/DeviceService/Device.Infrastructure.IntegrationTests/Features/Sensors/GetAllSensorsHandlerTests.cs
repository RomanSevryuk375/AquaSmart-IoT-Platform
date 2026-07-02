using Device.Application.Features.Sensors.Query.GetAllSensors;
using Device.Application.Features.Sensors.Query.Shared;
using Device.Domain.Entities.Sensors;

namespace Device.Infrastructure.IntegrationTests.Features.Sensors;

public class GetAllSensorsHandlerTests(
    IntegrationTestWebAppFactory factory) : BaseIntegrationTest(factory)
{
    [Fact]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "<Pending>")]
    public async Task GetAll_IsolatesByUserId_And_FiltersByType()
    {
        // Arrange
        var myUserId = Guid.NewGuid();
        var myControllerId = Guid.NewGuid();

        var hackerUserId = Guid.NewGuid();
        var hackerControllerId = Guid.NewGuid();

        Controller myController = new ControllerBuilder()
            .WithId(myControllerId)
            .WithUserId(myUserId)
            .WithMacAddress("AA:AA:AA:AA:AA:AA")
            .Build();

        Controller hackerController = new ControllerBuilder()
            .WithId(hackerControllerId)
            .WithUserId(hackerUserId)
            .WithMacAddress("BB:BB:BB:BB:BB:BB")
            .WithDeviceTokenHash("hacker_token_hash")
            .Build();

        Sensor myTempSensor1 = new SensorBuilder()
            .WithId(Guid.NewGuid())
            .WithUserId(myUserId)
            .WithControllerId(myControllerId)
            .WithType(SensorType.Temperature)
            .WithAddress(ConnectionProtocol.I2C, "0x01")
            .Build();

        Sensor myTempSensor2 = new SensorBuilder()
            .WithId(Guid.NewGuid())
            .WithUserId(myUserId)
            .WithControllerId(myControllerId)
            .WithType(SensorType.Temperature)
            .WithAddress(ConnectionProtocol.I2C, "0x02")
            .Build();

        Sensor myHumSensor = new SensorBuilder()
            .WithId(Guid.NewGuid())
            .WithUserId(myUserId)
            .WithControllerId(myControllerId)
            .WithType(SensorType.Humidity)
            .WithAddress(ConnectionProtocol.I2C, "0x03")
            .Build();

        Sensor hackerSensor = new SensorBuilder()
            .WithId(Guid.NewGuid())
            .WithUserId(hackerUserId)
            .WithControllerId(hackerControllerId)
            .WithType(SensorType.Temperature)
            .WithAddress(ConnectionProtocol.I2C, "0x01")
            .Build();

        DbContext.Controllers.AddRange(myController, hackerController);
        DbContext.Sensors.AddRange(myTempSensor1, myTempSensor2, myHumSensor, hackerSensor);
        await DbContext.SaveChangesAsync();

        var query = new GetAllSensorsQuery
        {
            UserId = myUserId,
            Type = SensorType.Temperature,
            Skip = 0,
            Take = 100
        };

        // Act
        Result<IReadOnlyList<SensorDto>> result = await Sender.Send(query);

        // Assert
        result.IsSuccess.Should().BeTrue();
        IReadOnlyList<SensorDto> sensors = result.Value;

        sensors.Should().HaveCount(2);
        sensors.Should().AllSatisfy(s =>
        {
            s.Type.Should().Be(SensorType.Temperature);
            s.ControllerId.Should().Be(myControllerId);
        });

        SensorDto returnedSensor = sensors.First(s => s.Id == myTempSensor1.Id);
        returnedSensor.ConnectionProtocol.Should().Be(ConnectionProtocol.I2C);
        returnedSensor.ConnectionAddress.Should().Be("0x01");
    }

    [Fact]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "<Pending>")]
    public async Task GetAll_WithPagination_ReturnsCorrectSubset()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var controllerId = Guid.NewGuid();

        Controller controller = new ControllerBuilder()
            .WithId(controllerId)
            .WithUserId(userId)
            .Build();

        Sensor s1 = new SensorBuilder()
            .WithId(Guid.NewGuid())
            .WithUserId(userId)
            .WithControllerId(controllerId)
            .WithAddress(ConnectionProtocol.Digital, "D1")
            .Build();

        Sensor s2 = new SensorBuilder()
            .WithId(Guid.NewGuid())
            .WithUserId(userId)
            .WithControllerId(controllerId)
            .WithAddress(ConnectionProtocol.Digital, "D2")
            .Build();

        Sensor s3 = new SensorBuilder()
            .WithId(Guid.NewGuid())
            .WithUserId(userId)
            .WithControllerId(controllerId)
            .WithAddress(ConnectionProtocol.Digital, "D3")
            .Build();

        DbContext.Controllers.Add(controller);
        DbContext.Sensors.AddRange(s1, s2, s3);
        await DbContext.SaveChangesAsync();

        var query = new GetAllSensorsQuery
        {
            UserId = userId,
            Skip = 1,
            Take = 1
        };

        // Act
        Result<IReadOnlyList<SensorDto>> result = await Sender.Send(query);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(1);
    }
}
