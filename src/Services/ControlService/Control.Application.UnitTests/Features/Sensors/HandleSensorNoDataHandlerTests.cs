using Contracts.Events.RelayEvents;
using Contracts.Events.SensorEvents;
using Control.Application.Features.Sensors.Commands.HandleSensorNoData;
using MassTransit;

namespace Control.Application.UnitTests.Features.Sensors;

public class HandleSensorNoDataHandlerTests
{
    private readonly ISensorRepository _sensorRepoMock = Substitute.For<ISensorRepository>();
    private readonly IEcosystemRepository _ecosystemRepoMock = Substitute.For<IEcosystemRepository>();
    private readonly IAutomationRuleRepository _ruleRepoMock = Substitute.For<IAutomationRuleRepository>();
    private readonly IPublishEndpoint _publishEndpointMock = Substitute.For<IPublishEndpoint>();
    private readonly HandleSensorNoDataHandler _handler;

    public HandleSensorNoDataHandlerTests()
    {
        _handler = new HandleSensorNoDataHandler(
            _sensorRepoMock,
            _ecosystemRepoMock,
            _ruleRepoMock,
            _publishEndpointMock);
    }

    [Fact]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "<Pending>")]
    public async Task Handle_WhenSensorNotFound_ReturnsNotFoundFailure()
    {
        // Arrange
        _sensorRepoMock.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns((Sensor?)null);

        var command = new HandleSensorNoDataCommand
        {
            SensorId = Guid.NewGuid(),
            State = SensorState.NoData,
            LastSeenAt = DateTime.UtcNow
        };

        // Act
        Result result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Sensor.NotFound");
    }

    [Fact]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "<Pending>")]
    public async Task Handle_WhenEcosystemNotFound_ReturnsNotFoundFailure()
    {
        // Arrange
        Sensor sensor = new SensorBuilder().Build();
        _sensorRepoMock.GetByIdAsync(sensor.Id, Arg.Any<CancellationToken>()).Returns(sensor);
        _ecosystemRepoMock.GetByIdAsync(sensor.EcosystemId, Arg.Any<CancellationToken>()).Returns((Ecosystem?)null);

        var command = new HandleSensorNoDataCommand
        {
            SensorId = sensor.Id,
            State = SensorState.NoData,
            LastSeenAt = DateTime.UtcNow
        };

        // Act
        Result result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Ecosystem.NotFound");
    }

    [Fact]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "<Pending>")]
    public async Task Handle_WhenNoAffectedRules_UpdatesStateAndReturnsSuccessWithoutPublishing()
    {
        // Arrange
        Ecosystem ecosystem = new EcosystemBuilder().Build();
        Sensor sensor = new SensorBuilder().WithEcosystemId(ecosystem.Id).Build();

        _sensorRepoMock.GetByIdAsync(sensor.Id, Arg.Any<CancellationToken>()).Returns(sensor);
        _ecosystemRepoMock.GetByIdAsync(ecosystem.Id, Arg.Any<CancellationToken>()).Returns(ecosystem);
        _ruleRepoMock.GetBySensorIdWithConditionsAsync(sensor.Id, Arg.Any<CancellationToken>())
            .Returns([]);

        var command = new HandleSensorNoDataCommand
        {
            SensorId = sensor.Id,
            State = SensorState.NoData,
            LastSeenAt = DateTime.UtcNow
        };

        // Act
        Result result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        sensor.State.Should().Be(SensorState.Faulty);
        await _publishEndpointMock.DidNotReceiveWithAnyArgs().Publish(Arg.Any<object>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "<Pending>")]
    public async Task Handle_WhenAffectedRulesExist_UpdatesStateAndPublishesEvents()
    {
        // Arrange
        Ecosystem ecosystem = new EcosystemBuilder().Build();
        Sensor sensor = new SensorBuilder().WithEcosystemId(ecosystem.Id).Build();
        Relay relay = new RelayBuilder().WithEcosystemId(ecosystem.Id).Build();
        AutomationRule rule = new AutomationRuleBuilder()
            .WithEcosystemId(ecosystem.Id)
            .WithRelayId(relay.Id)
            .Build();

        _sensorRepoMock.GetByIdAsync(sensor.Id, Arg.Any<CancellationToken>()).Returns(sensor);
        _ecosystemRepoMock.GetByIdAsync(ecosystem.Id, Arg.Any<CancellationToken>()).Returns(ecosystem);
        _ruleRepoMock.GetBySensorIdWithConditionsAsync(sensor.Id, Arg.Any<CancellationToken>())
            .Returns([rule]);

        var command = new HandleSensorNoDataCommand
        {
            SensorId = sensor.Id,
            State = SensorState.NoData,
            LastSeenAt = DateTime.UtcNow.AddMinutes(-5)
        };

        // Act
        Result result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        sensor.State.Should().Be(SensorState.Faulty);

        await _publishEndpointMock.Received(1).Publish(
            Arg.Is<ChangeRelayStateEvent>(e =>
                e.ControllerId == ecosystem.ControllerId &&
                e.RelayId == rule.RelayId && !e.TargetState),
            Arg.Any<CancellationToken>());

        await _publishEndpointMock.Received(1).Publish(
            Arg.Is<SensorNoDataAlertEvent>(e =>
                e.UserId == ecosystem.UserId &&
                e.EcosytemId == rule.EcosystemId &&
                e.SensorId == sensor.Id &&
                e.LastSeenAt == command.LastSeenAt),
            Arg.Any<CancellationToken>());
    }
}
