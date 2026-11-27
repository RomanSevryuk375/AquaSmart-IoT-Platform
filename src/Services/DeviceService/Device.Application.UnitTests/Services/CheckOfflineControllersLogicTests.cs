using BuildingBlocks.Domain.Abstractions;
using BuildingBlocks.Domain.Results;
using Device.Application.Extesions;
using Device.Application.Services;
using Device.Domain.Events.ControllerEvents;
using Microsoft.Extensions.Options;

namespace Device.Application.UnitTests.Services;

public class CheckOfflineControllersLogicTests
{
    private readonly IControllerRepository _controllerRepoMock = Substitute.For<IControllerRepository>();
    private readonly IUnitOfWork _unitOfWorkMock = Substitute.For<IUnitOfWork>();

    [Fact]
    public async Task CheckOfflineControllers_WhenOfflineWithinGracePeriod_DoesNotRaiseAlert()
    {
        // Arrange
        IOptions<DeviceSettings> options = Options.Create(new DeviceSettings { OfflineAlertGraceSeconds = 120 });
        var service = new ControllerOfflineCheckerService(_controllerRepoMock, _unitOfWorkMock, options);

        _controllerRepoMock.GetOfflinePendingAlertAsync(Arg.Any<DateTime>(), Arg.Any<CancellationToken>())
            .Returns([]);
        _controllerRepoMock.GetOfflineControllersAsync(Arg.Any<DateTime>(), Arg.Any<CancellationToken>())
            .Returns([]);

        // Act
        Result<int> result = await service.CheckAndDisableControllerAsync(CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(0);
        await _unitOfWorkMock.DidNotReceiveWithAnyArgs().SaveChangesAsync(default);
    }

    [Fact]
    public async Task CheckOfflineControllers_WhenOfflineBeyondGracePeriod_RaisesSingleControllerNotOnlineEvent()
    {
        // Arrange
        IOptions<DeviceSettings> options = Options.Create(new DeviceSettings { OfflineAlertGraceSeconds = 120 });
        var service = new ControllerOfflineCheckerService(_controllerRepoMock, _unitOfWorkMock, options);

        Controller lwtOfflineController = new ControllerBuilder().AsOffline().Build();

        _controllerRepoMock.GetOfflinePendingAlertAsync(Arg.Any<DateTime>(), Arg.Any<CancellationToken>())
            .Returns([lwtOfflineController]);
        _controllerRepoMock.GetOfflineControllersAsync(Arg.Any<DateTime>(), Arg.Any<CancellationToken>())
            .Returns([]);

        // Act
        Result<int> result = await service.CheckAndDisableControllerAsync(CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(1);
        lwtOfflineController.OfflineAlertSent.Should().BeTrue();
        lwtOfflineController.DomainEvents.Should().ContainSingle()
            .Which.Should().BeOfType<ControllerNotOnlineDomainEvent>()
            .Which.ControllerId.Should().Be(lwtOfflineController.Id);
        await _unitOfWorkMock.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task CheckOfflineControllers_WhenAlreadyAlerted_DoesNotRaiseDuplicateAlert()
    {
        // Arrange
        IOptions<DeviceSettings> options = Options.Create(new DeviceSettings { OfflineAlertGraceSeconds = 120 });
        var service = new ControllerOfflineCheckerService(_controllerRepoMock, _unitOfWorkMock, options);

        Controller alreadyAlerted = new ControllerBuilder().AsOffline().Build();
        alreadyAlerted.MarkOfflineAlertSent();
        alreadyAlerted.ClearDomainEvents();

        _controllerRepoMock.GetOfflinePendingAlertAsync(Arg.Any<DateTime>(), Arg.Any<CancellationToken>())
            .Returns([alreadyAlerted]);
        _controllerRepoMock.GetOfflineControllersAsync(Arg.Any<DateTime>(), Arg.Any<CancellationToken>())
            .Returns([]);

        // Act
        Result<int> result = await service.CheckAndDisableControllerAsync(CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        alreadyAlerted.DomainEvents.Should().BeEmpty();
    }

    [Fact]
    public async Task CheckOfflineControllers_WhenOnlineButSilentOver5Min_MarksOfflineAndRaisesAlert()
    {
        // Arrange
        IOptions<DeviceSettings> options = Options.Create(new DeviceSettings { OfflineAlertGraceSeconds = 120 });
        var service = new ControllerOfflineCheckerService(_controllerRepoMock, _unitOfWorkMock, options);

        Controller silentOnlineController = new ControllerBuilder().Build();

        _controllerRepoMock.GetOfflinePendingAlertAsync(Arg.Any<DateTime>(), Arg.Any<CancellationToken>())
            .Returns([]);
        _controllerRepoMock.GetOfflineControllersAsync(Arg.Any<DateTime>(), Arg.Any<CancellationToken>())
            .Returns([silentOnlineController]);

        // Act
        Result<int> result = await service.CheckAndDisableControllerAsync(CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(1);
        silentOnlineController.IsOnline.Should().BeFalse();
        silentOnlineController.OfflineAlertSent.Should().BeTrue();
        silentOnlineController.DomainEvents.Should().ContainSingle()
            .Which.Should().BeOfType<ControllerNotOnlineDomainEvent>();
        await _unitOfWorkMock.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}
