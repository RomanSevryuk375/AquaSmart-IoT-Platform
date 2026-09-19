using BuildingBlocks.Domain.Results;
using Device.Application.Services;

namespace Device.Application.UnitTests.Services;

public class DeviceSecurityServiceTests
{
    private readonly IControllerRepository _controllerRepoMock = Substitute.For<IControllerRepository>();
    private readonly IDeviceTokenHasher _tokenHasherMock = Substitute.For<IDeviceTokenHasher>();
    private readonly DeviceSecurityService _service;

    public DeviceSecurityServiceTests()
    {
        _service = new DeviceSecurityService(_controllerRepoMock, _tokenHasherMock);
    }

    [Fact]
    public async Task EnsureUserOwnsControllerAsync_WhenControllerNotFound_ReturnsNotFound()
    {
        // Arrange
        var controllerId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        _controllerRepoMock.GetByIdAsync(controllerId, Arg.Any<CancellationToken>())
            .Returns((Controller?)null);

        // Act
        Result result = await _service.EnsureUserOwnsControllerAsync(controllerId, userId);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Controller.NotFound");
    }

    [Fact]
    public async Task EnsureUserOwnsControllerAsync_WhenUserDoesNotOwnController_ReturnsForbidden()
    {
        // Arrange
        var controllerId = Guid.NewGuid();
        var ownerId = Guid.NewGuid();
        var requestingUserId = Guid.NewGuid();

        Controller controller = new ControllerBuilder().WithUserId(ownerId).Build();

        _controllerRepoMock.GetByIdAsync(controllerId, Arg.Any<CancellationToken>())
            .Returns(controller);

        // Act
        Result result = await _service.EnsureUserOwnsControllerAsync(controllerId, requestingUserId);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Type.Should().Be(ErrorType.Forbidden);
    }

    [Fact]
    public async Task EnsureUserOwnsControllerAsync_WhenUserOwnsController_ReturnsSuccess()
    {
        // Arrange
        var controllerId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        Controller controller = new ControllerBuilder().WithUserId(userId).Build();

        _controllerRepoMock.GetByIdAsync(controllerId, Arg.Any<CancellationToken>())
            .Returns(controller);

        // Act
        Result result = await _service.EnsureUserOwnsControllerAsync(controllerId, userId);

        // Assert
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task EnsureDeviceAccessAsync_WhenControllerNotFound_ReturnsNotFound()
    {
        // Arrange
        var controllerId = Guid.NewGuid();

        _controllerRepoMock.GetByIdAsync(controllerId, Arg.Any<CancellationToken>())
            .Returns((Controller?)null);

        // Act
        Result result = await _service.EnsureDeviceAccessAsync(controllerId, "raw_token");

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Controller.NotFound");
    }

    [Fact]
    public async Task EnsureDeviceAccessAsync_WhenTokenInvalid_ReturnsUnauthorized()
    {
        // Arrange
        var controllerId = Guid.NewGuid();
        const string rawToken = "invalid_token";
        Controller controller = new ControllerBuilder().Build();

        _controllerRepoMock.GetByIdAsync(controllerId, Arg.Any<CancellationToken>())
            .Returns(controller);
        _tokenHasherMock.Verify(rawToken, controller.DeviceTokenHash).Returns(false);

        // Act
        Result result = await _service.EnsureDeviceAccessAsync(controllerId, rawToken);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Type.Should().Be(ErrorType.Unauthorized);
    }

    [Fact]
    public async Task EnsureDeviceAccessAsync_WhenTokenValid_ReturnsSuccess()
    {
        // Arrange
        var controllerId = Guid.NewGuid();
        const string rawToken = "valid_token";
        Controller controller = new ControllerBuilder().Build();

        _controllerRepoMock.GetByIdAsync(controllerId, Arg.Any<CancellationToken>())
            .Returns(controller);
        _tokenHasherMock.Verify(rawToken, controller.DeviceTokenHash).Returns(true);

        // Act
        Result result = await _service.EnsureDeviceAccessAsync(controllerId, rawToken);

        // Assert
        result.IsSuccess.Should().BeTrue();
    }
}
