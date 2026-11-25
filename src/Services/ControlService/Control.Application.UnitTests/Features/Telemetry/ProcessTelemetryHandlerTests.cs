using BuildingBlocks.Domain.Enums;
using BuildingBlocks.Domain.Results;
using Control.Application.Features.Relays.Commands.SyncRelayMode;
using Control.Application.Features.Telemetry.Commands.ProcessTelemetry;
using Control.Domain.Events;

namespace Control.Application.UnitTests.Features.Telemetry;

public class ProcessTelemetryHandlerTests
{
    private readonly IAutomationRuleRepository _ruleRepoMock = Substitute.For<IAutomationRuleRepository>();
    private readonly ISensorRepository _sensorRepoMock = Substitute.For<ISensorRepository>();
    private readonly IRelayRepository _relayRepoMock = Substitute.For<IRelayRepository>();
    private readonly ProcessTelemetryHandler _handler;

    public ProcessTelemetryHandlerTests()
    {
        _handler = new ProcessTelemetryHandler(_ruleRepoMock, _sensorRepoMock, _relayRepoMock);
    }

    [Fact]
    public async Task Handle_WhenRelayIsManual_DoesNotTriggerRelayStateChange()
    {
        // Arrange
        var ecosystemId = Guid.NewGuid();
        var sensorId = Guid.NewGuid();
        var relayId = Guid.NewGuid();

        Sensor sensor = new SensorBuilder()
            .WithId(sensorId)
            .WithEcosystemId(ecosystemId)
            .Build();

        Relay relay = new RelayBuilder()
            .WithId(relayId)
            .WithEcosystemId(ecosystemId)
            .WithIsManual(true)
            .WithIsActive(false)
            .Build();

        RuleCondition condition = new RuleConditionBuilder()
            .WithSensorId(sensorId)
            .WithCondition(Condition.Greater)
            .WithThreshold(25.0)
            .WithHysteresis(0.5)
            .Build();

        AutomationRule rule = new AutomationRuleBuilder()
            .WithEcosystemId(ecosystemId)
            .WithRelayId(relayId)
            .WithIsActive(true)
            .WithAction(RuleAction.SwitchOn)
            .WithOperator(Operator.AND)
            .WithCondition(condition)
            .Build();

        _sensorRepoMock.GetByIdAsync(sensorId, Arg.Any<CancellationToken>()).Returns(sensor);
        _ruleRepoMock.GetBySensorIdWithConditionsAsync(sensorId, Arg.Any<CancellationToken>()).Returns([rule]);
        _sensorRepoMock.GetManyByIdsAsync(Arg.Any<IEnumerable<Guid>>(), Arg.Any<CancellationToken>()).Returns([sensor]);
        _relayRepoMock.GetManyByIds(Arg.Any<IEnumerable<Guid>>(), Arg.Any<CancellationToken>()).Returns([relay]);

        var command = new ProcessTelemetryCommand
        {
            SensorId = sensorId,
            Value = 28.0,
            RecordedAt = DateTime.UtcNow
        };

        // Act
        Result result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        relay.IsActive.Should().BeFalse();
        relay.DomainEvents.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_AfterRelaySyncedBackToAuto_ResumesAutomationAndRaisesStateChange()
    {
        // Arrange
        var ecosystemId = Guid.NewGuid();
        var sensorId = Guid.NewGuid();
        var relayId = Guid.NewGuid();

        Sensor sensor = new SensorBuilder()
            .WithId(sensorId)
            .WithEcosystemId(ecosystemId)
            .Build();

        Relay relay = new RelayBuilder()
            .WithId(relayId)
            .WithEcosystemId(ecosystemId)
            .WithIsManual(true)
            .WithIsActive(false)
            .Build();

        RuleCondition condition = new RuleConditionBuilder()
            .WithSensorId(sensorId)
            .WithCondition(Condition.Greater)
            .WithThreshold(25.0)
            .WithHysteresis(0.5)
            .Build();

        AutomationRule rule = new AutomationRuleBuilder()
            .WithEcosystemId(ecosystemId)
            .WithRelayId(relayId)
            .WithIsActive(true)
            .WithAction(RuleAction.SwitchOn)
            .WithOperator(Operator.AND)
            .WithCondition(condition)
            .Build();

        _sensorRepoMock.GetByIdAsync(sensorId, Arg.Any<CancellationToken>()).Returns(sensor);
        _ruleRepoMock.GetBySensorIdWithConditionsAsync(sensorId, Arg.Any<CancellationToken>()).Returns([rule]);
        _sensorRepoMock.GetManyByIdsAsync(Arg.Any<IEnumerable<Guid>>(), Arg.Any<CancellationToken>()).Returns([sensor]);
        _relayRepoMock.GetManyByIds(Arg.Any<IEnumerable<Guid>>(), Arg.Any<CancellationToken>()).Returns([relay]);
        _relayRepoMock.GetByIdAsync(relayId, Arg.Any<CancellationToken>()).Returns(relay);

        var syncModeHandler = new SyncRelayModeHandler(_relayRepoMock);
        await syncModeHandler.Handle(new SyncRelayModeCommand { RelayId = relayId, IsManual = false }, CancellationToken.None);

        var command = new ProcessTelemetryCommand
        {
            SensorId = sensorId,
            Value = 28.0,
            RecordedAt = DateTime.UtcNow
        };

        // Act
        Result result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        relay.IsManual.Should().BeFalse();
        relay.IsActive.Should().BeTrue();
        relay.DomainEvents.Should().ContainSingle().Which.Should().BeOfType<RelayStateChangedDomainEvent>();
    }
}
