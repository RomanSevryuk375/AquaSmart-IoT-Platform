using IdentityService.Application.Features.Profile.Commands.ChangePassword;
using Microsoft.AspNetCore.Identity;

namespace Identity.Application.UnitTests.Features.Profile.Commands.ChangePassword;

public class ChangePasswordHandlerTests
{
    private readonly UserManager<User> _userManagerMock;
    private readonly IUserContext _userContextMock = Substitute.For<IUserContext>();
    private readonly ChangePasswordHandler _handler;

    public ChangePasswordHandlerTests()
    {
        IUserStore<User> storeMock = Substitute.For<IUserStore<User>>();
        _userManagerMock = Substitute.For<UserManager<User>>(storeMock, null, null, null, null, null, null, null, null);
        _handler = new ChangePasswordHandler(_userManagerMock, _userContextMock);
    }

    [Fact]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "<Pending>")]
    public async Task Handle_WhenUserNotFound_ReturnsNotFoundError()
    {
        // Arrange
        var userId = Guid.NewGuid();
        _userContextMock.UserId.Returns(userId);
        _userManagerMock.FindByIdAsync(userId.ToString()).Returns((User?)null);

        var command = new ChangePasswordCommand
        {
            CurrentPassword = "OldPassword123!",
            NewPassword = "NewPassword123!"
        };

        // Act
        Result result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("User.NotFound");
        result.Error.Message.Should().Be("User not found.");

        await _userManagerMock.DidNotReceive()
            .ChangePasswordAsync(Arg.Any<User>(), Arg.Any<string>(), Arg.Any<string>());
    }

    [Fact]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "<Pending>")]
    public async Task Handle_WithValidRequest_ChangesPasswordAndReturnsSuccess()
    {
        // Arrange
        User user = new UserBuilder().Build();
        _userContextMock.UserId.Returns(user.Id);
        _userManagerMock.FindByIdAsync(user.Id.ToString()).Returns(user);

        _userManagerMock.ChangePasswordAsync(user, "OldPassword123!", "NewPassword123!")
            .Returns(IdentityResult.Success);

        var command = new ChangePasswordCommand
        {
            CurrentPassword = "OldPassword123!",
            NewPassword = "NewPassword123!"
        };

        // Act
        Result result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();

        await _userManagerMock.Received(1).ChangePasswordAsync(user, "OldPassword123!", "NewPassword123!");
    }

    [Fact]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "<Pending>")]
    public async Task Handle_WhenChangePasswordFails_ReturnsValidationError()
    {
        // Arrange
        User user = new UserBuilder().Build();
        _userContextMock.UserId.Returns(user.Id);
        _userManagerMock.FindByIdAsync(user.Id.ToString()).Returns(user);

        var identityError = new IdentityError { Description = "Password must contain at least one digit." };
        _userManagerMock.ChangePasswordAsync(user, "OldPassword123!", "weakpassword")
            .Returns(IdentityResult.Failed(identityError));

        var command = new ChangePasswordCommand
        {
            CurrentPassword = "OldPassword123!",
            NewPassword = "weakpassword"
        };

        // Act
        Result result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Password.ChangeFailed");
        result.Error.Message.Should().Be("Password must contain at least one digit.");

        await _userManagerMock.Received(1).ChangePasswordAsync(user, "OldPassword123!", "weakpassword");
    }
}
