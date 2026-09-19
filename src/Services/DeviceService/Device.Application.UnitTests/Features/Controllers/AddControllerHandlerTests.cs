using BuildingBlocks.Domain.Results;
using Device.Application.Features.Controllers.Command.AddController;

namespace Device.Application.UnitTests.Features.Controllers;

public class AddControllerHandlerTests
{
    private readonly IDeviceTokenHasher _hasherMock = Substitute.For<IDeviceTokenHasher>();
    private readonly IControllerRepository _controllerRepoMock = Substitute.For<IControllerRepository>();
    private readonly AddControllerHandler _handler;

    public AddControllerHandlerTests()
    {
        _handler = new AddControllerHandler(_hasherMock, _controllerRepoMock);
    }

    [Fact]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "<Pending>")]
    public async Task Handle_WithValidCommand_GeneratesRawTokenHashesItAndSaves()
    {
        // Arrange
        var userId = Guid.NewGuid();

        string expectedRawToken = "ak_test_raw_token_12345";
        string expectedHash = "some_generated_hmac_hash";

        _hasherMock.GenerateRawToken().Returns(expectedRawToken);
        _hasherMock.ComputeHash(expectedRawToken).Returns(expectedHash);

        var command = new AddControllerCommand
        {
            UserId = userId,
            MacAddress = TestConstants.ValidMacAddress,
            Name = "New Controller",
            IsOnline = true
        };

        // Act
        Result<ControllerRegisteredResponse> result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.DeviceToken.Should().Be(expectedRawToken);
        result.Value.ControllerId.Should().NotBeEmpty();

        await _controllerRepoMock.Received(1).AddAsync(
            Arg.Is<Controller>(c => c.DeviceTokenHash == expectedHash &&
                                    c.MacAddress.Value == TestConstants.ValidMacAddress),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "<Pending>")]
    public async Task Handle_WithDomainValidationError_ReturnsFailureAndDoesNotSave()
    {
        var userId = Guid.NewGuid();
        // Arrange
        var command = new AddControllerCommand
        {

            UserId = userId
,
            MacAddress = "invalid_mac",
            Name = "New Controller",
            IsOnline = true
        };

        // Act
        Result<ControllerRegisteredResponse> result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();

        await _controllerRepoMock.DidNotReceiveWithAnyArgs().AddAsync(default!, default);
    }
}
