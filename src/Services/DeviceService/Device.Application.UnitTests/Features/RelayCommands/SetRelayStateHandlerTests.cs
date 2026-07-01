using Contracts.Results;
using Device.Application.Features.RelayCommands.Command.SetRelayState;
using Device.Domain.Entities;
using Device.Domain.Interfaces;
using Device.TestShared.Builders;
using FluentAssertions;
using NSubstitute;

namespace Device.Application.UnitTests.Features.RelayCommands;

public class SetRelayStateHandlerTests
{
    private readonly IRelayRepository _relayRepoMock = Substitute.For<IRelayRepository>();
    private readonly IControllerRepository _controllerRepoMock = Substitute.For<IControllerRepository>();
    private readonly IRelayCommandsRepository _queueRepoMock = Substitute.For<IRelayCommandsRepository>();
    private readonly IUnitOfWork _unitOfWorkMock = Substitute.For<IUnitOfWork>();
    private readonly SetRelayStateHandler _handler;

    public SetRelayStateHandlerTests()
    {
        _handler = new SetRelayStateHandler(_relayRepoMock, _controllerRepoMock, _queueRepoMock, _unitOfWorkMock);
    }

    [Fact]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "<Pending>")]
    public async Task Handle_WithValidCommand_CreatesCommandAndSaves()
    {
        // Arrange
        Relay relay = new RelayBuilder().AsAuto().AsActive(false).Build();
        Controller controller = new ControllerBuilder().WithId(relay.ControllerId).Build();

        _relayRepoMock.GetByIdAsync(relay.Id, Arg.Any<CancellationToken>()).Returns(relay);
        _controllerRepoMock.GetByIdAsync(controller.Id, Arg.Any<CancellationToken>()).Returns(controller);

        var command = new SetRelayStateCommand
        {
            ControllerId = controller.Id,
            RelayId = relay.Id,
            TargetState = true,
            ExpireAt = DateTime.UtcNow.AddMinutes(5)
        };

        // Act
        Result result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();

        await _queueRepoMock.Received(1).AddAsync(Arg.Is<RelayCommand>(c =>
            c.RelayId == relay.Id && c.TargeState),
            Arg.Any<CancellationToken>());

        await _unitOfWorkMock.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "<Pending>")]
    public async Task Handle_WhenRelayIsManual_ReturnsConflictFailure()
    {
        // Arrange
        Relay relay = new RelayBuilder().Build();
        Controller controller = new ControllerBuilder().WithId(relay.ControllerId).Build();

        _relayRepoMock.GetByIdAsync(relay.Id, Arg.Any<CancellationToken>()).Returns(relay);
        _controllerRepoMock.GetByIdAsync(controller.Id, Arg.Any<CancellationToken>()).Returns(controller);

        var command = new SetRelayStateCommand
        {
            ControllerId = controller.Id,
            RelayId = relay.Id,
            TargetState = !relay.IsActive
        };

        // Act
        Result result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Message.Should().Contain("unavailable or was expired");
        await _queueRepoMock.DidNotReceiveWithAnyArgs().AddAsync(default!, default);
    }

    [Fact]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "<Pending>")]
    public async Task Handle_WhenTargetStateEqualsCurrentState_ReturnsConflictFailure()
    {
        // Arrange
        Relay relay = new RelayBuilder().AsAuto().AsActive(true).Build();
        Controller controller = new ControllerBuilder().WithId(relay.ControllerId).Build();

        _relayRepoMock.GetByIdAsync(relay.Id, Arg.Any<CancellationToken>()).Returns(relay);
        _controllerRepoMock.GetByIdAsync(controller.Id, Arg.Any<CancellationToken>()).Returns(controller);

        var command = new SetRelayStateCommand
        {
            ControllerId = controller.Id,
            RelayId = relay.Id,
            TargetState = true
        };

        // Act
        Result result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        await _queueRepoMock.DidNotReceiveWithAnyArgs().AddAsync(default!, default);
    }

    [Fact]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "<Pending>")]
    public async Task Handle_WhenCommandIsExpired_ReturnsConflictFailure()
    {
        // Arrange
        Relay relay = new RelayBuilder().AsAuto().AsActive(false).Build();
        Controller controller = new ControllerBuilder().WithId(relay.ControllerId).Build();

        _relayRepoMock.GetByIdAsync(relay.Id, Arg.Any<CancellationToken>()).Returns(relay);
        _controllerRepoMock.GetByIdAsync(controller.Id, Arg.Any<CancellationToken>()).Returns(controller);

        var command = new SetRelayStateCommand
        {
            ControllerId = controller.Id,
            RelayId = relay.Id,
            TargetState = true,
            ExpireAt = DateTime.UtcNow.AddMinutes(-10)
        };

        // Act
        Result result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        await _queueRepoMock.DidNotReceiveWithAnyArgs().AddAsync(default!, default);
    }
}
