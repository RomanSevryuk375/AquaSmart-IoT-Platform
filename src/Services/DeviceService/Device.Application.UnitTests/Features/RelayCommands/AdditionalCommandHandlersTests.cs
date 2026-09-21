using BuildingBlocks.Domain.Enums;
using Device.Application.Features.Controllers.Command.PingController;
using Device.Application.Features.Controllers.Command.ToggleCommandState;
using Device.Application.Features.RelayCommands.Command.DeleteCompleted;
using Device.Application.Features.RelayCommands.Command.MarkAsCompleted;
using Device.Application.Features.RelayCommands.Command.MarkAsFailed;
using Device.Application.Features.RelayCommands.Command.ToggleRelayMode;
using ZiggyCreatures.Caching.Fusion;

namespace Device.Application.UnitTests.Features.RelayCommands;

public class AdditionalCommandHandlersTests
{
    private readonly IRelayCommandsRepository _queueRepoMock = Substitute.For<IRelayCommandsRepository>();
    private readonly IRelayRepository _relayRepoMock = Substitute.For<IRelayRepository>();
    private readonly IControllerRepository _controllerRepoMock = Substitute.For<IControllerRepository>();
    private readonly IFusionCache _cacheMock = Substitute.For<IFusionCache>();

    [Fact]
    public async Task MarkAsCompletedHandler_WhenCommandNotFound_ReturnsNotFound()
    {
        var handler = new MarkAsCompletedHandler(_queueRepoMock, _relayRepoMock);
        var commandId = Guid.NewGuid();
        _queueRepoMock.GetByIdAsync(commandId, Arg.Any<CancellationToken>()).Returns((RelayCommand?)null);

        var result = await handler.Handle(new MarkAsCompletedCommand { CommandId = commandId, DeviceToken = "tok" }, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public async Task MarkAsCompletedHandler_WhenRelayNotFound_ReturnsNotFound()
    {
        var handler = new MarkAsCompletedHandler(_queueRepoMock, _relayRepoMock);
        RelayCommand relayCmd = new RelayCommandBuilder().Build();
        _queueRepoMock.GetByIdAsync(relayCmd.Id, Arg.Any<CancellationToken>()).Returns(relayCmd);
        _relayRepoMock.GetByIdAsync(relayCmd.RelayId, Arg.Any<CancellationToken>()).Returns((Relay?)null);

        var result = await handler.Handle(new MarkAsCompletedCommand { CommandId = relayCmd.Id, DeviceToken = "tok" }, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public async Task MarkAsCompletedHandler_WhenValid_MarksCompletedAndReturnsSuccess()
    {
        var handler = new MarkAsCompletedHandler(_queueRepoMock, _relayRepoMock);
        RelayCommand relayCmd = new RelayCommandBuilder().Build();
        Relay relay = new RelayBuilder().Build();

        _queueRepoMock.GetByIdAsync(relayCmd.Id, Arg.Any<CancellationToken>()).Returns(relayCmd);
        _relayRepoMock.GetByIdAsync(relayCmd.RelayId, Arg.Any<CancellationToken>()).Returns(relay);

        var result = await handler.Handle(new MarkAsCompletedCommand { CommandId = relayCmd.Id, DeviceToken = "tok" }, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        relayCmd.Status.Should().Be(CommandStatus.Completed);
    }

    [Fact]
    public async Task MarkAsFailedHandler_WhenCommandNotFound_ReturnsNotFound()
    {
        var handler = new MarkAsFailedHandler(_queueRepoMock);
        var commandId = Guid.NewGuid();
        _queueRepoMock.GetByIdAsync(commandId, Arg.Any<CancellationToken>()).Returns((RelayCommand?)null);

        var result = await handler.Handle(new MarkAsFailedCommand { CommandId = commandId, DeviceToken = "tok", ErrorMessage = "fail" }, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public async Task MarkAsFailedHandler_WhenValid_MarksFailedAndReturnsSuccess()
    {
        var handler = new MarkAsFailedHandler(_queueRepoMock);
        RelayCommand relayCmd = new RelayCommandBuilder().Build();
        _queueRepoMock.GetByIdAsync(relayCmd.Id, Arg.Any<CancellationToken>()).Returns(relayCmd);

        var result = await handler.Handle(new MarkAsFailedCommand { CommandId = relayCmd.Id, DeviceToken = "tok", ErrorMessage = "fail" }, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        relayCmd.Status.Should().Be(CommandStatus.Failed);
    }

    [Fact]
    public async Task DeleteCompletedHandler_WhenCalled_ReturnsDeletedCount()
    {
        var handler = new DeleteCompletedHandler(_queueRepoMock);
        _queueRepoMock.DeleteCompletedAsync(Arg.Any<CancellationToken>()).Returns(5);

        var result = await handler.Handle(new DeleteCompletedCommand(), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(5);
    }

    [Fact]
    public async Task ToggleRelayModeHandler_WhenValid_TogglesMode()
    {
        var handler = new ToggleRelayModeHandler(_relayRepoMock);
        Relay relay = new RelayBuilder().Build(); // IsManual = true by default
        _relayRepoMock.GetByIdAsync(relay.Id, Arg.Any<CancellationToken>()).Returns(relay);

        var result = await handler.Handle(new ToggleRelayModeCommand { RelayId = relay.Id, UserId = relay.UserId }, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        relay.IsManual.Should().BeFalse();
    }

    [Fact]
    public async Task ToggleControllerStateHandler_WhenNotFound_ReturnsNotFound()
    {
        var handler = new ToggleControllerStateHandler(_controllerRepoMock, _cacheMock);
        var controllerId = Guid.NewGuid();
        _controllerRepoMock.GetByIdAsync(controllerId, Arg.Any<CancellationToken>()).Returns((Controller?)null);

        var result = await handler.Handle(new ToggleControllerStateCommand { ControllerId = controllerId, UserId = Guid.NewGuid() }, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public async Task ToggleControllerStateHandler_WhenValid_TogglesStateAndClearsCache()
    {
        var handler = new ToggleControllerStateHandler(_controllerRepoMock, _cacheMock);
        Controller controller = new ControllerBuilder().Build(); // IsOnline = true by default
        _controllerRepoMock.GetByIdAsync(controller.Id, Arg.Any<CancellationToken>()).Returns(controller);

        var result = await handler.Handle(new ToggleControllerStateCommand { ControllerId = controller.Id, UserId = controller.UserId }, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        controller.IsOnline.Should().BeFalse();
        await _cacheMock.Received(1).RemoveAsync(Arg.Any<string>(), token: Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task PingControllerHandler_WhenNotFound_ReturnsNotFound()
    {
        var handler = new PingControllerHandler(_controllerRepoMock);
        var controllerId = Guid.NewGuid();
        _controllerRepoMock.GetByIdAsync(controllerId, Arg.Any<CancellationToken>()).Returns((Controller?)null);

        var result = await handler.Handle(new PingControllerCommand { ControllerId = controllerId, DeviceToken = "tok" }, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public async Task PingControllerHandler_WhenValid_RecordsPingAndReturnsSuccess()
    {
        var handler = new PingControllerHandler(_controllerRepoMock);
        Controller controller = new ControllerBuilder().Build();
        _controllerRepoMock.GetByIdAsync(controller.Id, Arg.Any<CancellationToken>()).Returns(controller);

        var result = await handler.Handle(new PingControllerCommand { ControllerId = controller.Id, DeviceToken = "tok" }, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
    }
}
