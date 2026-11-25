using BuildingBlocks.Domain.Enums;
using BuildingBlocks.Domain.Results;
using Device.Application.Features.Controllers.Command.UpdateController;
using Device.Application.Features.Controllers.Query.GetControllerConfig;
using Device.Application.Features.Integrations.ValidateDeviceToken;
using Device.Application.Features.Integrations.ValidateDeviceTokenByControllerId;
using Device.Application.Features.Sensors.Command.AddSensor;
using Device.Application.Interfaces;
using Device.Domain.Interfaces;

namespace Device.Infrastructure.IntegrationTests.Mqtt;

public class MqttPersistenceAndJobsTests(
    IntegrationTestWebAppFactory factory) : BaseIntegrationTest(factory)
{
    [Fact]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "<Pending>")]
    public async Task MigrationColumns_ControllerDiagnosticsAndTerminalCommandStatuses_PersistCorrectly()
    {
        // Arrange
        Controller controller = new ControllerBuilder()
            .WithId(Guid.NewGuid())
            .WithMacAddress("AA:11:22:33:44:55")
            .Build();
        controller.RecordMqttOnline("1.4.0", "10.0.0.42", -55, 7200);
        controller.IncrementConfigVersion("test_migration");
        controller.MarkOfflineFromLwt();
        controller.MarkOfflineAlertSent();

        Relay relay = new RelayBuilder()
            .WithId(Guid.NewGuid())
            .WithControllerId(controller.Id)
            .Build();

        RelayCommand cancelledCmd = new RelayCommandBuilder()
            .WithId(Guid.NewGuid())
            .WithControllerId(controller.Id)
            .WithRelayId(relay.Id)
            .Build();
        cancelledCmd.Cancel();

        RelayCommand expiredCmd = new RelayCommandBuilder()
            .WithId(Guid.NewGuid())
            .WithControllerId(controller.Id)
            .WithRelayId(relay.Id)
            .Build();
        expiredCmd.Expire();

        DbContext.Controllers.Add(controller);
        DbContext.Relays.Add(relay);
        DbContext.RelayCommands.AddRange(cancelledCmd, expiredCmd);
        await DbContext.SaveChangesAsync();
        DbContext.ChangeTracker.Clear();

        // Act
        Controller? dbController = await DbContext.Controllers
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == controller.Id);
        List<RelayCommand> dbCommands = await DbContext.RelayCommands
            .AsNoTracking()
            .Where(c => c.ControllerId == controller.Id)
            .ToListAsync();

        // Assert
        dbController.Should().NotBeNull();
        dbController!.ConfigVersion.Should().Be(2);
        dbController.FirmwareVersion.Should().Be("1.4.0");
        dbController.IpAddress.Should().Be("10.0.0.42");
        dbController.Rssi.Should().Be(-55);
        dbController.UptimeSec.Should().Be(7200);
        dbController.OfflineAlertSent.Should().BeTrue();
        dbController.IsOnline.Should().BeFalse();

        dbCommands.Should().ContainSingle(c => c.Id == cancelledCmd.Id && c.Status == CommandStatus.Cancelled);
        dbCommands.Should().ContainSingle(c => c.Id == expiredCmd.Id && c.Status == CommandStatus.Expired);
    }

    [Fact]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "<Pending>")]
    public async Task GetControllerConfigHandler_AfterSensorAddition_ReturnsIncrementedConfigVersion()
    {
        // Arrange
        Controller controller = new ControllerBuilder()
            .WithId(Guid.NewGuid())
            .WithMacAddress("AA:11:22:33:44:66")
            .Build();

        DbContext.Controllers.Add(controller);
        await DbContext.SaveChangesAsync();
        DbContext.ChangeTracker.Clear();

        // Act 1: Add sensor via MediatR command
        Result<SensorCreatedResponse> addSensorResult = await Sender.Send(new AddSensorCommand
        {
            ControllerId = controller.Id,
            UserId = controller.UserId,
            Name = "Tank Temp",
            ConnectionProtocol = ConnectionProtocol.OneWire,
            ConnectionAddress = "28FF4A1B2C3D4E5F",
            Type = SensorType.Temperature
        });

        // Act 2: Query config using compact MAC format
        Result<ControllerConfig> configResult = await Sender.Send(new GetControllerConfigQuery
        {
            MacAddress = "AA1122334466"
        });

        // Assert
        addSensorResult.IsSuccess.Should().BeTrue();
        configResult.IsSuccess.Should().BeTrue();
        configResult.Value.ConfigVersion.Should().Be(2);
        configResult.Value.Sensors.Should().HaveCount(1);
    }

    [Fact]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "<Pending>")]
    public async Task CancelPendingAndSentByRelayId_CancelsOnlyTargetRelayActiveCommands()
    {
        // Arrange
        Controller controller = new ControllerBuilder()
            .WithId(Guid.NewGuid())
            .WithMacAddress("AA:11:22:33:44:77")
            .Build();

        Relay targetRelay = new RelayBuilder().WithId(Guid.NewGuid()).WithControllerId(controller.Id).Build();
        Relay otherRelay = new RelayBuilder().WithId(Guid.NewGuid()).WithControllerId(controller.Id).WithPurpose(RelayPurpose.Light).Build();

        RelayCommand pendingTarget = new RelayCommandBuilder().WithId(Guid.NewGuid()).WithControllerId(controller.Id).WithRelayId(targetRelay.Id).Build();
        RelayCommand sentTarget = new RelayCommandBuilder().WithId(Guid.NewGuid()).WithControllerId(controller.Id).WithRelayId(targetRelay.Id).Build();
        sentTarget.MarkAsSent();
        RelayCommand completedTarget = new RelayCommandBuilder().WithId(Guid.NewGuid()).WithControllerId(controller.Id).WithRelayId(targetRelay.Id).Build();
        completedTarget.MarkAsCompleted();
        RelayCommand failedTarget = new RelayCommandBuilder().WithId(Guid.NewGuid()).WithControllerId(controller.Id).WithRelayId(targetRelay.Id).Build();
        failedTarget.MarkAsFailed("err");
        RelayCommand pendingOther = new RelayCommandBuilder().WithId(Guid.NewGuid()).WithControllerId(controller.Id).WithRelayId(otherRelay.Id).Build();

        DbContext.Controllers.Add(controller);
        DbContext.Relays.AddRange(targetRelay, otherRelay);
        DbContext.RelayCommands.AddRange(pendingTarget, sentTarget, completedTarget, failedTarget, pendingOther);
        await DbContext.SaveChangesAsync();
        DbContext.ChangeTracker.Clear();

        // Act
        IRelayCommandsRepository repo = GetRequiredService<IRelayCommandsRepository>();
        IReadOnlyList<RelayCommand> active = await repo.GetActiveByRelayIdAsync(targetRelay.Id);
        foreach (RelayCommand cmd in active)
        {
            cmd.Cancel();
        }

        await DbContext.SaveChangesAsync();
        DbContext.ChangeTracker.Clear();

        // Assert
        List<RelayCommand> dbCommands = await DbContext.RelayCommands.AsNoTracking().ToListAsync();
        dbCommands.First(c => c.Id == pendingTarget.Id).Status.Should().Be(CommandStatus.Cancelled);
        dbCommands.First(c => c.Id == sentTarget.Id).Status.Should().Be(CommandStatus.Cancelled);
        dbCommands.First(c => c.Id == completedTarget.Id).Status.Should().Be(CommandStatus.Completed);
        dbCommands.First(c => c.Id == failedTarget.Id).Status.Should().Be(CommandStatus.Failed);
        dbCommands.First(c => c.Id == pendingOther.Id).Status.Should().Be(CommandStatus.Pending);
    }

    [Fact]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "<Pending>")]
    public async Task RetryRelayCommands_WhenExpiredInDb_MarksStatusExpiredInPostgres()
    {
        // Arrange
        Controller controller = new ControllerBuilder()
            .WithId(Guid.NewGuid())
            .WithMacAddress("AA:11:22:33:44:88")
            .Build();
        Relay relay = new RelayBuilder().WithId(Guid.NewGuid()).WithControllerId(controller.Id).Build();

        RelayCommand expiredCommand = new RelayCommandBuilder()
            .WithId(Guid.NewGuid())
            .WithControllerId(controller.Id)
            .WithRelayId(relay.Id)
            .WithExpireAt(DateTime.UtcNow.AddMinutes(-5))
            .Build();

        DbContext.Controllers.Add(controller);
        DbContext.Relays.Add(relay);
        DbContext.RelayCommands.Add(expiredCommand);
        await DbContext.SaveChangesAsync();
        DbContext.ChangeTracker.Clear();

        // Act
        IRelayCommandRetryService retryService = GetRequiredService<IRelayCommandRetryService>();
        Result<int> result = await retryService.ProcessActiveCommandsAsync(CancellationToken.None);
        DbContext.ChangeTracker.Clear();

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(1);
        RelayCommand? dbCmd = await DbContext.RelayCommands.AsNoTracking().FirstOrDefaultAsync(c => c.Id == expiredCommand.Id);
        dbCmd.Should().NotBeNull();
        dbCmd!.Status.Should().Be(CommandStatus.Expired);
        dbCmd.ProcessedAt.Should().NotBeNull();
    }

    [Fact]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "<Pending>")]
    public async Task ValidateTokenByControllerId_WithValidAndMismatchedController_ValidatesAndEvictsCacheOnUpdate()
    {
        // Arrange
        IDeviceTokenHasher tokenHasher = GetRequiredService<IDeviceTokenHasher>();
        string rawTokenA = "ak_controller_a_secret_token";
        string rawTokenB = "ak_controller_b_secret_token";

        Controller controllerA = new ControllerBuilder()
            .WithId(Guid.NewGuid())
            .WithMacAddress("AA:11:22:33:44:91")
            .WithDeviceTokenHash(tokenHasher.ComputeHash(rawTokenA))
            .Build();
        Controller controllerB = new ControllerBuilder()
            .WithId(Guid.NewGuid())
            .WithMacAddress("AA:11:22:33:44:92")
            .WithDeviceTokenHash(tokenHasher.ComputeHash(rawTokenB))
            .Build();

        DbContext.Controllers.AddRange(controllerA, controllerB);
        await DbContext.SaveChangesAsync();
        DbContext.ChangeTracker.Clear();

        // Act 1: Valid controllerId + token
        Result<ValidateDeviceTokenDto> validRes = await Sender.Send(new ValidateDeviceTokenByControllerIdQuery
        {
            ControllerId = controllerA.Id,
            RawDeviceToken = rawTokenA
        });

        // Act 2: IDOR — controllerA token with controllerB ID
        Result<ValidateDeviceTokenDto> idorRes = await Sender.Send(new ValidateDeviceTokenByControllerIdQuery
        {
            ControllerId = controllerB.Id,
            RawDeviceToken = rawTokenA
        });

        // Act 3: Update controllerA (evicts cache) and rotate hash in DB directly, then re-verify
        Result updateRes = await Sender.Send(new UpdateControllerCommand
        {
            ControllerId = controllerA.Id,
            UserId = controllerA.UserId,
            MacAddress = "AA:11:22:33:44:93",
            Name = "Updated Controller A"
        });

        string newHash = tokenHasher.ComputeHash("ak_rotated_secret_token");
        await DbContext.Database.ExecuteSqlRawAsync(
            "UPDATE controllers SET device_token_hash = {0} WHERE id = {1}",
            newHash,
            controllerA.Id);

        Result<ValidateDeviceTokenDto> postUpdateOldToken = await Sender.Send(new ValidateDeviceTokenByControllerIdQuery
        {
            ControllerId = controllerA.Id,
            RawDeviceToken = rawTokenA
        });
        Result<ValidateDeviceTokenDto> postUpdateNewToken = await Sender.Send(new ValidateDeviceTokenByControllerIdQuery
        {
            ControllerId = controllerA.Id,
            RawDeviceToken = "ak_rotated_secret_token"
        });

        // Assert
        validRes.IsSuccess.Should().BeTrue();
        validRes.Value.IsValid.Should().BeTrue();
        validRes.Value.ControllerId.Should().Be(controllerA.Id);
        validRes.Value.UserId.Should().Be(controllerA.UserId);

        idorRes.IsSuccess.Should().BeTrue();
        idorRes.Value.IsValid.Should().BeFalse();

        updateRes.IsSuccess.Should().BeTrue();
        postUpdateOldToken.Value.IsValid.Should().BeFalse();
        postUpdateNewToken.Value.IsValid.Should().BeTrue();
    }
}
