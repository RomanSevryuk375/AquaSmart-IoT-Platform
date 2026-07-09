namespace Control.Domain.UnitTests.Entities;

public class EcosystemTests
{
    [Fact]
    public void Create_WithValidData_ReturnsSuccessAndRaisesEcosystemCreatedDomainEvent()
    {
        // Arrange
        var id = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var controllerId = Guid.NewGuid();
        EcosystemType type = EcosystemType.Aquarium;
        string name = "My Freshwater Tank";
        double volume = 200.0;

        // Act
        Result<Ecosystem> result = Ecosystem.Create(id, userId, type, name, volume, controllerId);

        // Assert
        result.IsSuccess.Should().BeTrue();
        Ecosystem ecosystem = result.Value;
        ecosystem.Id.Should().Be(id);
        ecosystem.UserId.Should().Be(userId);
        ecosystem.ControllerId.Should().Be(controllerId);
        ecosystem.Type.Should().Be(type);
        ecosystem.Name.Value.Should().Be(name);
        ecosystem.Volume.Should().NotBeNull();
        ecosystem.Volume!.Value.Should().Be(volume);

        EcosystemCreatedDomainEvent ev = ecosystem.DomainEvents.Should().ContainSingle()
            .Which.Should().BeOfType<EcosystemCreatedDomainEvent>()
            .Subject;
        ev.EcosystemId.Should().Be(id);
        ev.Name.Should().Be(name);
        ev.UserId.Should().Be(userId);
        ev.ControllerId.Should().Be(controllerId);
    }

    [Fact]
    public void Create_WithInvalidName_ReturnsFailure()
    {
        // Arrange
        var id = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var controllerId = Guid.NewGuid();

        // Act
        Result<Ecosystem> result = Ecosystem.Create(
            id, userId, EcosystemType.Terrarium, "", 100.0, controllerId);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Ecosystem.Invalid");
        result.Error.Message.Should().Contain("Name cannot be empty");
    }

    [Fact]
    public void Create_WithInvalidVolume_ReturnsFailure()
    {
        // Arrange
        var id = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var controllerId = Guid.NewGuid();

        // Act
        Result<Ecosystem> result = Ecosystem.Create(
            id, userId, EcosystemType.Terrarium, "Test", -10.0, controllerId);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Ecosystem.Invalid");
        result.Error.Message.Should().Contain("Volume must be strictly greater than zero");
    }

    [Fact]
    public void Create_WithMultipleErrors_ReturnsCombinedFailureMessage()
    {
        // Arrange
        var id = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var controllerId = Guid.NewGuid();

        // Act
        Result<Ecosystem> result = Ecosystem.Create(
            id, userId, EcosystemType.Terrarium, "", -10.0, controllerId);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Ecosystem.Invalid");
        result.Error.Message.Should().Contain("Name cannot be empty");
        result.Error.Message.Should().Contain("Volume must be strictly greater than zero");
    }

    [Fact]
    public void SetName_WithValidName_UpdatesNameIncrementsVersionAndRaisesEcosystemUpdatedDomainEvent()
    {
        // Arrange
        Ecosystem ecosystem = new EcosystemBuilder().Build();
        Guid initialVersion = ecosystem.Version;
        string newName = "Updated Ecosystem Name";

        // Act
        Result result = ecosystem.SetName(newName);

        // Assert
        result.IsSuccess.Should().BeTrue();
        ecosystem.Name.Value.Should().Be(newName);
        ecosystem.Version.Should().NotBe(initialVersion);

        EcosystemUpdatedDomainEvent ev = ecosystem.DomainEvents.Should().ContainSingle()
            .Which.Should().BeOfType<EcosystemUpdatedDomainEvent>()
            .Subject;
        ev.EcosystemId.Should().Be(ecosystem.Id);
        ev.UserId.Should().Be(ecosystem.UserId);
        ev.Name.Should().Be(newName);
        ev.ControllerId.Should().Be(ecosystem.ControllerId);
        ev.CreatedAt.Should().Be(ecosystem.CreatedAt);
    }

    [Fact]
    public void SetName_WithInvalidName_ReturnsFailureAndDoesNotIncrementVersionOrRaiseEvent()
    {
        // Arrange
        Ecosystem ecosystem = new EcosystemBuilder().Build();
        Guid initialVersion = ecosystem.Version;

        // Act
        Result result = ecosystem.SetName("");

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Name.Invalid");
        ecosystem.Version.Should().Be(initialVersion);
        ecosystem.DomainEvents.Should().BeEmpty();
    }

    [Fact]
    public void SetVolume_WithValidVolume_UpdatesVolumeAndIncrementsVersion()
    {
        // Arrange
        Ecosystem ecosystem = new EcosystemBuilder().Build();
        Guid initialVersion = ecosystem.Version;
        double? newVolume = 150.0;

        // Act
        Result result = ecosystem.SetVolume(newVolume);

        // Assert
        result.IsSuccess.Should().BeTrue();
        ecosystem.Volume.Should().NotBeNull();
        ecosystem.Volume!.Value.Should().Be(150.0);
        ecosystem.Version.Should().NotBe(initialVersion);
    }

    [Fact]
    public void SetVolume_WithNullVolume_UpdatesVolumeToNullAndIncrementsVersion()
    {
        // Arrange
        Ecosystem ecosystem = new EcosystemBuilder().Build();
        Guid initialVersion = ecosystem.Version;

        // Act
        Result result = ecosystem.SetVolume(null);

        // Assert
        result.IsSuccess.Should().BeTrue();
        ecosystem.Volume.Should().BeNull();
        ecosystem.Version.Should().NotBe(initialVersion);
    }

    [Fact]
    public void SetVolume_WithInvalidVolume_ReturnsFailureAndDoesNotIncrementVersion()
    {
        // Arrange
        Ecosystem ecosystem = new EcosystemBuilder().Build();
        Guid initialVersion = ecosystem.Version;

        // Act
        Result result = ecosystem.SetVolume(-20.0);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Volume.Invalid");
        ecosystem.Version.Should().Be(initialVersion);
    }
}
