using BuildingBlocks.Domain.Results;

namespace Device.Domain.UnitTests.Entities;

public class ControllerTests
{
    [Fact]
    public void Create_WithValidData_ReturnsSuccess()
    {
        // Act
        Result<Controller> result = Controller.Create(
            TestConstants.ControllerId,
            TestConstants.UserId,
            TestConstants.ValidMacAddress,
            TestConstants.ValidTokenHash,
            TestConstants.ValidDeviceName,
            isOnline: true);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Id.Should().Be(TestConstants.ControllerId);
        result.Value.UserId.Should().Be(TestConstants.UserId);
        result.Value.MacAddress.Value.Should().Be(TestConstants.ValidMacAddress);
        result.Value.DeviceTokenHash.Should().Be(TestConstants.ValidTokenHash);
        result.Value.Name.Value.Should().Be(TestConstants.ValidDeviceName);
        result.Value.IsOnline.Should().BeTrue();
    }

    [Fact]
    public void Create_WithEmptyUserId_ReturnsValidationFailure()
    {
        // Act
        Result<Controller> result = Controller.Create(
            TestConstants.ControllerId,
            Guid.Empty,
            TestConstants.ValidMacAddress,
            TestConstants.ValidTokenHash,
            TestConstants.ValidDeviceName,
            isOnline: true);

        // Assert
        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public void Update_WithValidData_UpdatesPropertiesAndReturnsSuccess()
    {
        // Arrange
        Controller controller = new ControllerBuilder().Build();

        string newMac = "11:22:33:44:55:66";
        string newName = "Updated Controller";

        // Act
        Result result = controller.Update(newMac, newName);

        // Assert
        result.IsSuccess.Should().BeTrue();
        controller.MacAddress.Value.Should().Be(newMac);
        controller.Name.Value.Should().Be(newName);
    }

    [Fact]
    public void Update_WithInvalidData_ReturnsFailureAndDoesNotChangeProperties()
    {
        // Arrange
        Controller controller = new ControllerBuilder().Build();

        // Act
        Result result = controller.Update("invalid_mac", "");

        // Assert
        result.IsFailure.Should().BeTrue();
        controller.MacAddress.Value.Should().Be(TestConstants.ValidMacAddress);
        controller.Name.Value.Should().Be(TestConstants.ValidDeviceName);
    }

    [Fact]
#pragma warning disable IDE1006 // Naming Styles
    public async Task RecordPing_UpdatesLastSeenAt()
#pragma warning restore IDE1006 // Naming Styles
    {
        // Arrange
        Controller controller = new ControllerBuilder().Build();
        DateTime initialLastSeen = controller.LastSeenAt;

        await Task.Delay(1);

        // Act
        controller.RecordPing();

        // Assert
        controller.LastSeenAt.Should().BeAfter(initialLastSeen);
    }

    [Fact]
    public void ToggleState_FromOnlineToOffline_RaisesNotOnlineEvent()
    {
        // Arrange
        Controller controller = new ControllerBuilder().Build();

        // Act
        controller.ToggleState();

        // Assert
        controller.IsOnline.Should().BeFalse();
        controller.DomainEvents.Should().ContainSingle()
            .Which.Should().BeOfType<ControllerNotOnlineDomainEvent>()
            .Which.Should().Match<ControllerNotOnlineDomainEvent>(e =>
                e.ControllerId == TestConstants.ControllerId &&
                e.UserId == TestConstants.UserId);
    }

    [Fact]
    public void ToggleState_FromOfflineToOnline_DoesNotRaiseEvent()
    {
        // Arrange 
        Controller controller = new ControllerBuilder().AsOffline().Build();

        // Act
        controller.ToggleState();

        // Assert
        controller.IsOnline.Should().BeTrue();
        controller.DomainEvents.Should().BeEmpty();
    }

    [Fact]
    public void SetOffline_WhenOnline_ChangesStateAndRaisesEvent()
    {
        // Arrange
        Controller controller = new ControllerBuilder().Build();

        // Act
        controller.SetOffline();

        // Assert
        controller.IsOnline.Should().BeFalse();
        controller.OfflineAlertSent.Should().BeTrue();
        controller.DomainEvents.Should().ContainSingle()
            .Which.Should().BeOfType<ControllerNotOnlineDomainEvent>();
    }

    [Fact]
    public void IncrementConfigVersion_OnMutation_IncrementsConfigVersionVersionAndRaisesEvent()
    {
        // Arrange
        Controller controller = new ControllerBuilder().Build();
        long initialConfigVersion = controller.ConfigVersion;
        Guid initialVersion = controller.Version;

        // Act
        controller.IncrementConfigVersion("sensor_added");

        // Assert
        controller.ConfigVersion.Should().Be(initialConfigVersion + 1);
        controller.Version.Should().NotBe(initialVersion);
        controller.DomainEvents.Should().ContainSingle()
            .Which.Should().BeOfType<ControllerConfigChangedDomainEvent>()
            .Which.Should().Match<ControllerConfigChangedDomainEvent>(e =>
                e.ControllerId == controller.Id &&
                e.MacAddress == controller.MacAddress.Value &&
                e.ConfigVersion == initialConfigVersion + 1 &&
                e.Reason == "sensor_added");
    }

    [Fact]
    public void RecordMqttOnline_WithDiagnostics_UpdatesOnlineLastSeenDiagnosticsAndResetsAlertFlag()
    {
        // Arrange
        Controller controller = new ControllerBuilder().Build();
        controller.SetOffline();
        controller.ClearDomainEvents();
        Guid versionBefore = controller.Version;

        // Act
        controller.RecordMqttOnline("1.2.3", "192.168.1.45", -58, 1240);

        // Assert
        controller.IsOnline.Should().BeTrue();
        controller.OfflineAlertSent.Should().BeFalse();
        controller.FirmwareVersion.Should().Be("1.2.3");
        controller.IpAddress.Should().Be("192.168.1.45");
        controller.Rssi.Should().Be(-58);
        controller.UptimeSec.Should().Be(1240);
        controller.Version.Should().NotBe(versionBefore);
        controller.DomainEvents.Should().BeEmpty();
    }

    [Fact]
    public void MarkOfflineFromLwt_WhenOnline_SetsOfflineWithoutRaisingAlertEventAndPreservesLastSeenAt()
    {
        // Arrange
        Controller controller = new ControllerBuilder().Build();
        DateTime lastSeenBefore = controller.LastSeenAt;
        Guid versionBefore = controller.Version;

        // Act
        controller.MarkOfflineFromLwt();

        // Assert
        controller.IsOnline.Should().BeFalse();
        controller.OfflineAlertSent.Should().BeFalse();
        controller.LastSeenAt.Should().Be(lastSeenBefore);
        controller.Version.Should().NotBe(versionBefore);
        controller.DomainEvents.Should().BeEmpty();
    }

    [Fact]
    public void MarkOfflineFromLwt_WhenAlreadyOffline_IsIdempotentDoesNotChangeVersionOrRaiseEvents()
    {
        // Arrange
        Controller controller = new ControllerBuilder().AsOffline().Build();
        Guid versionBefore = controller.Version;
        DateTime lastSeenBefore = controller.LastSeenAt;

        // Act
        controller.MarkOfflineFromLwt();

        // Assert
        controller.IsOnline.Should().BeFalse();
        controller.LastSeenAt.Should().Be(lastSeenBefore);
        controller.Version.Should().Be(versionBefore);
        controller.DomainEvents.Should().BeEmpty();
    }

    [Fact]
    public void MarkOfflineAlertSent_WhenNotSent_SetsFlagAndRaisesControllerNotOnlineDomainEvent()
    {
        // Arrange
        Controller controller = new ControllerBuilder().AsOffline().Build();
        Guid versionBefore = controller.Version;

        // Act
        controller.MarkOfflineAlertSent();

        // Assert
        controller.OfflineAlertSent.Should().BeTrue();
        controller.Version.Should().NotBe(versionBefore);
        controller.DomainEvents.Should().ContainSingle()
            .Which.Should().BeOfType<ControllerNotOnlineDomainEvent>()
            .Which.Should().Match<ControllerNotOnlineDomainEvent>(e =>
                e.ControllerId == controller.Id &&
                e.UserId == controller.UserId &&
                e.LastSeenAt == controller.LastSeenAt);
    }

    [Fact]
    public void MarkOfflineAlertSent_WhenAlreadySent_IsIdempotentNoOp()
    {
        // Arrange
        Controller controller = new ControllerBuilder().AsOffline().Build();
        controller.MarkOfflineAlertSent();
        controller.ClearDomainEvents();
        Guid versionBefore = controller.Version;

        // Act
        controller.MarkOfflineAlertSent();

        // Assert
        controller.OfflineAlertSent.Should().BeTrue();
        controller.Version.Should().Be(versionBefore);
        controller.DomainEvents.Should().BeEmpty();
    }
}
