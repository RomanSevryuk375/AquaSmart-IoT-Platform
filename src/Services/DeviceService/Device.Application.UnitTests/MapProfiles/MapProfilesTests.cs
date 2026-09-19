using AutoMapper;
using BuildingBlocks.Domain.Enums;
using BuildingBlocks.IntegrationEvents.Events.Controllers;
using BuildingBlocks.IntegrationEvents.Events.Relays;
using BuildingBlocks.IntegrationEvents.Events.Sensors;
using Device.Application.Features.Controllers.Query.GetControllerConfig;
using Device.Application.Features.RelayCommands.Command.SetRelayState;
using Device.Application.Features.RelayCommands.Query.GetPending;
using Device.Application.Features.Relays.Command.AddRelay;
using Device.Application.Features.Sensors.Command.AddSensor;
using Device.Application.Features.Sensors.Command.SetSensorState;
using Device.Application.MapProfiles;
using Device.Domain.Events.ControllerEvents;
using Device.Domain.Events.RelayEvents;
using Device.Domain.Events.SensorEvents;

namespace Device.Application.UnitTests.MapProfiles;

public class MapProfilesTests
{
    private readonly IMapper _mapper;

    public MapProfilesTests()
    {
        var loggerFactory = Substitute.For<Microsoft.Extensions.Logging.ILoggerFactory>();
        var config = new MapperConfiguration(cfg =>
        {
            cfg.AddProfile<ControllerProfile>();
            cfg.AddProfile<RelayProfile>();
            cfg.AddProfile<SensorProfile>();
        }, loggerFactory);

        _mapper = config.CreateMapper();
    }

    [Fact]
    public void ControllerProfile_MapsControllerNotOnlineDomainEvent_To_ControllerNotOnlineEvent()
    {
        var domainEvent = new ControllerNotOnlineDomainEvent
        {
            UserId = Guid.NewGuid(),
            ControllerId = Guid.NewGuid(),
            LastSeenAt = DateTime.UtcNow
        };

        var mapped = _mapper.Map<ControllerNotOnlineEvent>(domainEvent);

        mapped.Should().NotBeNull();
        mapped.UserId.Should().Be(domainEvent.UserId);
        mapped.ControllerId.Should().Be(domainEvent.ControllerId);
    }

    [Fact]
    public void RelayProfile_MapsRelayToRelayCreatedResponse()
    {
        Relay relay = new RelayBuilder().Build();
        var response = _mapper.Map<RelayCreatedResponse>(relay);

        response.Should().NotBeNull();
        response.Id.Should().Be(relay.Id);
    }

    [Fact]
    public void RelayProfile_MapsRelayToRelayCreatedEvent()
    {
        Relay relay = new RelayBuilder().Build();
        var mapped = _mapper.Map<RelayCreatedEvent>(relay);

        mapped.Should().NotBeNull();
        mapped.RelayId.Should().Be(relay.Id);
    }

    [Fact]
    public void RelayProfile_MapsRelayToRelayUpdatedEvent()
    {
        Relay relay = new RelayBuilder().Build();
        var mapped = _mapper.Map<RelayUpdatedEvent>(relay);

        mapped.Should().NotBeNull();
        mapped.RelayId.Should().Be(relay.Id);
    }

    [Fact]
    public void RelayProfile_MapsRelayToRelayConfig()
    {
        Relay relay = new RelayBuilder().Build();
        var mapped = _mapper.Map<RelayConfig>(relay);

        mapped.Should().NotBeNull();
        mapped.RelayId.Should().Be(relay.Id);
    }

    [Fact]
    public void RelayProfile_MapsRelayCommandToRelayCommandDto()
    {
        RelayCommand command = new RelayCommandBuilder().Build();
        var mapped = _mapper.Map<RelayCommandDto>(command);

        mapped.Should().NotBeNull();
        mapped.Id.Should().Be(command.Id);
    }

    [Fact]
    public void RelayProfile_MapsDomainEventsToIntegrationEvents()
    {
        var created = _mapper.Map<RelayCreatedEvent>(new RelayCreatedDomainEvent { RelayId = Guid.NewGuid() });
        created.Should().NotBeNull();

        var mode = _mapper.Map<RelayModeChangedEvent>(new RelayModeChangedDomainEvent { RelayId = Guid.NewGuid(), IsManual = true });
        mode.Should().NotBeNull();

        var state = _mapper.Map<ChangeRelayStateEvent>(new RelayStateChangedDomainEvent { RelayId = Guid.NewGuid(), TargetState = true });
        state.Should().NotBeNull();

        var updated = _mapper.Map<RelayUpdatedEvent>(new RelayUpdatedDomainEvent { RelayId = Guid.NewGuid() });
        updated.Should().NotBeNull();

        var setRelayCmd = _mapper.Map<SetRelayStateCommand>(new ChangeRelayStateEvent { RelayId = Guid.NewGuid(), TargetState = true });
        setRelayCmd.Should().NotBeNull();
    }

    [Fact]
    public void SensorProfile_MapsSensorToDTOsAndEvents()
    {
        Sensor sensor = new SensorBuilder().Build();

        var resp = _mapper.Map<SensorCreatedResponse>(sensor);
        resp.Should().NotBeNull();
        resp.Id.Should().Be(sensor.Id);

        var updEvent = _mapper.Map<SensorUpdatedEvent>(sensor);
        updEvent.Should().NotBeNull();
        updEvent.SensorId.Should().Be(sensor.Id);

        var stateEvent = _mapper.Map<SensorStateChangedEvent>(sensor);
        stateEvent.Should().NotBeNull();
        stateEvent.SensorId.Should().Be(sensor.Id);

        var createdEvent = _mapper.Map<SensorCreatedEvent>(sensor);
        createdEvent.Should().NotBeNull();
        createdEvent.SensorId.Should().Be(sensor.Id);

        var config = _mapper.Map<SensorConfig>(sensor);
        config.Should().NotBeNull();
        config.SensorId.Should().Be(sensor.Id);
    }

    [Fact]
    public void SensorProfile_MapsSensorDomainEventsToIntegrationEvents()
    {
        var created = _mapper.Map<SensorCreatedEvent>(new SensorCreatedDomainEvent { SensorId = Guid.NewGuid() });
        created.Should().NotBeNull();

        var state = _mapper.Map<SensorStateChangedEvent>(new SensorStateChangedDomainEvent { SensorId = Guid.NewGuid(), State = SensorState.Active });
        state.Should().NotBeNull();

        var updated = _mapper.Map<SensorUpdatedEvent>(new SensorUpdatedDomainEvent { SensorId = Guid.NewGuid() });
        updated.Should().NotBeNull();

        var pwr = _mapper.Map<SetRelayPowerSensorEvent>(new SetRelayPowerSensorDomainEvent { RelayId = Guid.NewGuid() });
        pwr.Should().NotBeNull();

        var noData = _mapper.Map<SetSensorStateCommand>(new SensorNoDataEvent { SensorId = Guid.NewGuid() });
        noData.Should().NotBeNull();
    }
}
