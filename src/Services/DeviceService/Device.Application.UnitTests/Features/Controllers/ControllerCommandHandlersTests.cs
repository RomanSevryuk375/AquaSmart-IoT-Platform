using BuildingBlocks.Domain.Abstractions;
using BuildingBlocks.Domain.Constants;
using BuildingBlocks.Domain.Results;
using Device.Application.Features.Controllers.Command.DeleteController;
using Device.Application.Features.Controllers.Command.UpdateController;
using Device.Application.Services;
using ZiggyCreatures.Caching.Fusion;

namespace Device.Application.UnitTests.Features.Controllers;

public class ControllerCommandHandlersTests
{
    private readonly IControllerRepository _controllerRepoMock = Substitute.For<IControllerRepository>();
    private readonly IFusionCache _cacheMock = Substitute.For<IFusionCache>();
    private readonly IUnitOfWork _unitOfWorkMock = Substitute.For<IUnitOfWork>();

    [Fact]
    public async Task UpdateControllerHandler_WhenNotFound_ReturnsNotFound()
    {
        // Arrange
        var handler = new UpdateControllerHandler(_controllerRepoMock, _cacheMock);
        var controllerId = Guid.NewGuid();

        _controllerRepoMock.GetByIdAsync(controllerId, Arg.Any<CancellationToken>())
            .Returns((Controller?)null);

        var command = new UpdateControllerCommand
        {
            ControllerId = controllerId,
            UserId = Guid.NewGuid(),
            MacAddress = TestConstants.ValidMacAddress,
            Name = "Updated Name"
        };

        // Act
        Result result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Controller.NotFound");
    }

    [Fact]
    public async Task UpdateControllerHandler_WhenValid_UpdatesControllerAndEvictsCache()
    {
        // Arrange
        var handler = new UpdateControllerHandler(_controllerRepoMock, _cacheMock);
        Controller controller = new ControllerBuilder().Build();

        _controllerRepoMock.GetByIdAsync(controller.Id, Arg.Any<CancellationToken>())
            .Returns(controller);

        var command = new UpdateControllerCommand
        {
            ControllerId = controller.Id,
            UserId = controller.UserId,
            MacAddress = "11:22:33:44:55:66",
            Name = "New Name"
        };

        // Act
        Result result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        controller.Name.Value.Should().Be("New Name");
        await _cacheMock.Received(1).RemoveAsync(Arg.Any<string>(), token: Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task DeleteControllerHandler_WhenValid_DeletesFromRepoAndEvictsCache()
    {
        // Arrange
        var handler = new DeleteControllerHandler(_controllerRepoMock, _cacheMock);
        var controllerId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        var command = new DeleteControllerCommand
        {
            ControllerId = controllerId,
            UserId = userId
        };

        // Act
        Result result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        await _controllerRepoMock.Received(1).DeleteAsync(controllerId, Arg.Any<CancellationToken>());
        await _cacheMock.Received(1).RemoveAsync(Arg.Any<string>(), token: Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ControllerOfflineCheckerService_WhenNoControllersOffline_ReturnsZero()
    {
        // Arrange
        var service = new ControllerOfflineCheckerService(_controllerRepoMock, _unitOfWorkMock);
        _controllerRepoMock.GetOfflineControllersAsync(Arg.Any<DateTime>(), Arg.Any<CancellationToken>())
            .Returns(new List<Controller>());

        // Act
        Result<int> result = await service.CheckAndDisableControllerAsync();

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(0);
        await _unitOfWorkMock.DidNotReceiveWithAnyArgs().SaveChangesAsync(default);
    }

    [Fact]
    public async Task ControllerOfflineCheckerService_WhenControllersFound_SetsOfflineAndSaves()
    {
        // Arrange
        var service = new ControllerOfflineCheckerService(_controllerRepoMock, _unitOfWorkMock);
        Controller controller1 = new ControllerBuilder().Build();
        Controller controller2 = new ControllerBuilder().Build();
        var offlineList = new List<Controller> { controller1, controller2 };

        _controllerRepoMock.GetOfflineControllersAsync(Arg.Any<DateTime>(), Arg.Any<CancellationToken>())
            .Returns(offlineList);

        // Act
        Result<int> result = await service.CheckAndDisableControllerAsync();

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(2);
        controller1.IsOnline.Should().BeFalse();
        controller2.IsOnline.Should().BeFalse();
        await _unitOfWorkMock.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ControllerOfflineCheckerService_WhenExceptionOccurs_ReturnsFailure()
    {
        // Arrange
        var service = new ControllerOfflineCheckerService(_controllerRepoMock, _unitOfWorkMock);
        _controllerRepoMock.GetOfflineControllersAsync(Arg.Any<DateTime>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromException<IReadOnlyList<Controller>>(new InvalidOperationException("DB connection error")));

        // Act
        Result<int> result = await service.CheckAndDisableControllerAsync();

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be(ErrorMessages.DatabaseError);
    }
}
