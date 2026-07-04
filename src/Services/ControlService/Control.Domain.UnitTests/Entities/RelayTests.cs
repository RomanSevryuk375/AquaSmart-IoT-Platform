namespace Control.Domain.UnitTests.Entities;

public class RelayTests
{
    [Fact]
    public void Create_WithValidData_ReturnsSuccess()
    {
        // Arrange
        var id = Guid.NewGuid();
        var ecosystemId = Guid.NewGuid();
        var controllerId = Guid.NewGuid();
        var powerSensorId = Guid.NewGuid();
        string name = "Heating Relay";
        RelayPurpose purpose = RelayPurpose.Heating;
        bool isManual = true;
        bool isActive = false;
        DateTime createdAt = DateTime.UtcNow;

        // Act
        Result<Relay> result = Relay.Create(
            id, ecosystemId, controllerId, powerSensorId, name, purpose, isManual, isActive, createdAt);

        // Assert
        result.IsSuccess.Should().BeTrue();
        Relay relay = result.Value;
        relay.Id.Should().Be(id);
        relay.EcosystemId.Should().Be(ecosystemId);
        relay.ControllerId.Should().Be(controllerId);
        relay.PowerSensorId.Should().Be(powerSensorId);
        relay.Name.Value.Should().Be(name);
        relay.Purpose.Should().Be(purpose);
        relay.IsManual.Should().Be(isManual);
        relay.IsActive.Should().Be(isActive);
        relay.CreatedAt.Should().Be(createdAt);
    }

    [Fact]
    public void Create_WithInvalidName_ReturnsFailure()
    {
        // Act
        Result<Relay> result = Relay.Create(
            relayId: Guid.NewGuid(),
            ecosystemId: Guid.NewGuid(),
            controllerId: Guid.NewGuid(),
            powerSensorId: null,
            rawName: "",
            RelayPurpose.Heating,
            isManual: true,
            isActive: false,
            createdAt: DateTime.UtcNow);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Name.Invalid");
    }

    [Fact]
    public void SetPurpose_WithNewPurpose_UpdatesPurposeAndIncrementsVersion()
    {
        // Arrange
        Relay relay = new RelayBuilder().WithPurpose(RelayPurpose.Heating).Build();
        Guid initialVersion = relay.Version;

        // Act
        relay.SetPurpose(RelayPurpose.Boiler);

        // Assert
        relay.Purpose.Should().Be(RelayPurpose.Boiler);
        relay.Version.Should().NotBe(initialVersion);
    }

    [Fact]
    public void SetPurpose_WithSamePurpose_DoesNotIncrementVersion()
    {
        // Arrange
        Relay relay = new RelayBuilder().WithPurpose(RelayPurpose.Heating).Build();
        Guid initialVersion = relay.Version;

        // Act
        relay.SetPurpose(RelayPurpose.Heating);

        // Assert
        relay.Purpose.Should().Be(RelayPurpose.Heating);
        relay.Version.Should().Be(initialVersion);
    }

    [Fact]
    public void SetMode_WithNewMode_UpdatesIsManualRaisesRelayModeChangedDomainEventAndIncrementsVersion()
    {
        // Arrange
        Relay relay = new RelayBuilder().WithIsManual(true).Build();
        Guid initialVersion = relay.Version;

        // Act
        relay.SetMode(false);

        // Assert
        relay.IsManual.Should().BeFalse();
        relay.Version.Should().NotBe(initialVersion);

        RelayModeChangedDomainEvent ev = relay.DomainEvents.Should().ContainSingle()
            .Which.Should().BeOfType<RelayModeChangedDomainEvent>()
            .Subject;
        ev.RelayId.Should().Be(relay.Id);
        ev.IsManual.Should().BeFalse();
    }

    [Fact]
    public void SetMode_WithSameMode_DoesNotIncrementVersionOrRaiseEvent()
    {
        // Arrange
        Relay relay = new RelayBuilder().WithIsManual(true).Build();
        Guid initialVersion = relay.Version;

        // Act
        relay.SetMode(true);

        // Assert
        relay.IsManual.Should().BeTrue();
        relay.Version.Should().Be(initialVersion);
        relay.DomainEvents.Should().BeEmpty();
    }

    [Fact]
    public void SetState_WithNewState_UpdatesIsActiveRaisesRelayStateChangedDomainEventAndIncrementsVersion()
    {
        // Arrange
        Relay relay = new RelayBuilder().WithIsActive(false).Build();
        Guid initialVersion = relay.Version;
        DateTime expireAt = DateTime.UtcNow.AddMinutes(30);

        // Act
        relay.SetState(true, expireAt);

        // Assert
        relay.IsActive.Should().BeTrue();
        relay.Version.Should().NotBe(initialVersion);

        RelayStateChangedDomainEvent ev = relay.DomainEvents.Should().ContainSingle()
            .Which.Should().BeOfType<RelayStateChangedDomainEvent>()
            .Subject;
        ev.ControllerId.Should().Be(relay.ControllerId);
        ev.RelayId.Should().Be(relay.Id);
        ev.TargetState.Should().BeTrue();
        ev.ExpireAt.Should().Be(expireAt);
    }

    [Fact]
    public void SetState_WithSameState_DoesNotIncrementVersionOrRaiseEvent()
    {
        // Arrange
        Relay relay = new RelayBuilder().WithIsActive(false).Build();
        Guid initialVersion = relay.Version;

        // Act
        relay.SetState(false, DateTime.UtcNow.AddMinutes(30));

        // Assert
        relay.IsActive.Should().Be(false);
        relay.Version.Should().Be(initialVersion);
        relay.DomainEvents.Should().BeEmpty();
    }

    [Fact]
    public void SetPowerSensorId_UpdatesPowerSensorIdAndIncrementsVersion()
    {
        // Arrange
        Relay relay = new RelayBuilder().WithPowerSensorId(null).Build();
        Guid initialVersion = relay.Version;
        var newSensorId = Guid.NewGuid();

        // Act
        relay.SetPowerSensorId(newSensorId);

        // Assert
        relay.PowerSensorId.Should().Be(newSensorId);
        relay.Version.Should().NotBe(initialVersion);
    }

    [Fact]
    public void SetName_WithValidName_UpdatesNameAndIncrementsVersion()
    {
        // Arrange
        Relay relay = new RelayBuilder().WithName("Old Name").Build();
        Guid initialVersion = relay.Version;

        // Act
        Result result = relay.SetName("New Name");

        // Assert
        result.IsSuccess.Should().BeTrue();
        relay.Name.Value.Should().Be("New Name");
        relay.Version.Should().NotBe(initialVersion);
    }

    [Fact]
    public void SetName_WithInvalidName_ReturnsFailureAndDoesNotIncrementVersion()
    {
        // Arrange
        Relay relay = new RelayBuilder().WithName("Old Name").Build();
        Guid initialVersion = relay.Version;

        // Act
        Result result = relay.SetName("");

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Name.Invalid");
        relay.Name.Value.Should().Be("Old Name");
        relay.Version.Should().Be(initialVersion);
    }
}
