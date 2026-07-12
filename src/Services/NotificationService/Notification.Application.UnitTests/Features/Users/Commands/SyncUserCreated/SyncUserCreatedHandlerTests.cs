using Contracts.Results;
using FluentAssertions;
using Notification.Application.Features.Users.Commands.SyncUserCreated;
using Notification.Domain.Entities;
using Notification.Domain.Interfaces;
using Notification.TestShared.Builders;
using NSubstitute;

namespace Notification.Application.UnitTests.Features.Users.Commands.SyncUserCreated;

public class SyncUserCreatedHandlerTests
{
    private readonly IUserRepository _userRepoMock = Substitute.For<IUserRepository>();
    private readonly SyncUserCreatedHandler _handler;

    public SyncUserCreatedHandlerTests()
    {
        _handler = new SyncUserCreatedHandler(_userRepoMock);
    }

    [Fact]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "<Pending>")]
    public async Task Handle_WhenUserAlreadyExists_ReturnsSuccessWithoutAdding()
    {
        // Arrange
        User user = new UserBuilder().Build();
        _userRepoMock.GetByIdAsync(user.Id, Arg.Any<CancellationToken>()).Returns(user);

        var command = new SyncUserCreatedCommand
        {
            UserId = user.Id,
            Email = user.Email.Value,
            PhoneNumber = user.PhoneNumber.Value,
            TimeZone = user.TimeZone.Value,
            CreatedAt = user.CreatedAt
        };

        // Act
        Result result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        await _userRepoMock.DidNotReceive().AddAsync(Arg.Any<User>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "<Pending>")]
    public async Task Handle_WhenUserDoesNotExistAndCommandIsValid_CreatesUserAndReturnsSuccess()
    {
        // Arrange
        var userId = Guid.NewGuid();
        _userRepoMock.GetByIdAsync(userId, Arg.Any<CancellationToken>()).Returns((User?)null);

        var command = new SyncUserCreatedCommand
        {
            UserId = userId,
            Email = "newuser@test.com",
            PhoneNumber = "+375291112233",
            TimeZone = "UTC",
            CreatedAt = DateTime.UtcNow
        };

        // Act
        Result result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();

        await _userRepoMock.Received(1).AddAsync(
            Arg.Is<User>(u =>
                u.Id == userId &&
                u.Email.Value == "newuser@test.com" &&
                u.PhoneNumber.Value == "+375291112233" &&
                u.TimeZone.Value == "UTC" &&
                !u.EmailEnable &&
                !u.TgEnable &&
                u.TelegramChatId == null &&
                !u.IsNotifyEnabled),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "<Pending>")]
    public async Task Handle_WhenUserDoesNotExistAndCommandIsInvalid_ReturnsFailure()
    {
        // Arrange
        var userId = Guid.NewGuid();
        _userRepoMock.GetByIdAsync(userId, Arg.Any<CancellationToken>()).Returns((User?)null);

        var command = new SyncUserCreatedCommand
        {
            UserId = userId,
            Email = "plainaddress",
            PhoneNumber = "+375291112233",
            TimeZone = "UTC",
            CreatedAt = DateTime.UtcNow
        };

        // Act
        Result result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("EmailAddress.Invalid");
        result.Error.Message.Should().Be("Invalid email address format.");

        await _userRepoMock.DidNotReceive().AddAsync(Arg.Any<User>(), Arg.Any<CancellationToken>());
    }
}
