using BuildingBlocks.Domain.Results;
using IdentityService.Application.Features.Profile.Commands.UpdateProfile;
using Microsoft.AspNetCore.Identity;

namespace Identity.Application.UnitTests.Features.Profile.Commands.UpdateProfile;

public class UpdateProfileHandlerTests
{
    private readonly UserManager<User> _userManagerMock;
    private readonly UpdateProfileHandler _handler;

    public UpdateProfileHandlerTests()
    {
        IUserStore<User> storeMock = Substitute.For<IUserStore<User>>();
        _userManagerMock = Substitute.For<UserManager<User>>(storeMock, null, null, null, null, null, null, null, null);
        _handler = new UpdateProfileHandler(_userManagerMock);
    }

    [Fact]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "<Pending>")]
    public async Task Handle_WhenUserNotFound_ReturnsNotFoundError()
    {
        // Arrange
        var userId = Guid.NewGuid();
        _userManagerMock.FindByIdAsync(userId.ToString()).Returns((User?)null);

        var command = new UpdateProfileCommand { UserId = userId, Name = "Jane Doe", PhoneNumber = "+375292223344" };

        // Act
        Result result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be(ErrorCodes.User.NotFound);
        result.Error.Message.Should().Be("User not found.");

        await _userManagerMock.DidNotReceive().UpdateAsync(Arg.Any<User>());
    }

    [Fact]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "<Pending>")]
    public async Task Handle_WithValidRequest_UpdatesProfileAndReturnsSuccess()
    {
        // Arrange
        User user = new UserBuilder().Build();
        _userManagerMock.FindByIdAsync(user.Id.ToString()).Returns(user);
        _userManagerMock.UpdateAsync(user).Returns(IdentityResult.Success);

        var command = new UpdateProfileCommand { UserId = user.Id, Name = "Jane Doe", PhoneNumber = "+375292223344" };

        // Act
        Result result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        user.Name.Value.Should().Be("Jane Doe");
        user.PhoneNumber.Should().Be("+375292223344");

        await _userManagerMock.Received(1).UpdateAsync(user);
    }

    [Fact]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "<Pending>")]
    public async Task Handle_WhenDomainValidationFails_ReturnsFailureAndDoesNotUpdate()
    {
        // Arrange
        User user = new UserBuilder().Build();
        _userManagerMock.FindByIdAsync(user.Id.ToString()).Returns(user);

        // Empty Name will fail domain validation
        var command = new UpdateProfileCommand { UserId = user.Id, Name = "", PhoneNumber = "+375292223344" };

        // Act
        Result result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("User.Invalid");

        await _userManagerMock.DidNotReceive().UpdateAsync(Arg.Any<User>());
    }

    [Fact]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "<Pending>")]
    public async Task Handle_WhenIdentityUpdateFails_ReturnsConflictError()
    {
        // Arrange
        User user = new UserBuilder().Build();
        _userManagerMock.FindByIdAsync(user.Id.ToString()).Returns(user);

        var identityError = new IdentityError { Description = "Database error updating user." };
        _userManagerMock.UpdateAsync(user).Returns(IdentityResult.Failed(identityError));

        var command = new UpdateProfileCommand { UserId = user.Id, Name = "Jane Doe", PhoneNumber = "+375292223344" };

        // Act
        Result result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be(ErrorCodes.Identity.ProfileUpdateFailed);
        result.Error.Message.Should().Be("Database error updating user.");

        await _userManagerMock.Received(1).UpdateAsync(user);
    }
}
