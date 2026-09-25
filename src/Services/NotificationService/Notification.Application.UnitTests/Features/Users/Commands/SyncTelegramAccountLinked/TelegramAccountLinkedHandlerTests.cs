using BuildingBlocks.Domain.Results;
using FluentAssertions;
using Notification.Application.Features.Users.Commands.SyncTelegramAccountLinked;
using Notification.Domain.Entities;
using Notification.Domain.Interfaces;
using Notification.TestShared.Builders;
using NSubstitute;

namespace Notification.Application.UnitTests.Features.Users.Commands.SyncTelegramAccountLinked;

public class TelegramAccountLinkedHandlerTests
{
    private readonly IUserRepository _userRepoMock = Substitute.For<IUserRepository>();
    private readonly TelegramAccountLinkedHandler _handler;

    public TelegramAccountLinkedHandlerTests()
    {
        _handler = new TelegramAccountLinkedHandler(_userRepoMock);
    }

    [Fact]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "<Pending>")]
    public async Task Handle_WhenUserExistsAndChatIdIsAvailable_LinksAccountAndReturnsSuccess()
    {
        // Arrange
        User user = new UserBuilder().WithTgEnable(false).Build();
        long chatId = 123456789L;

        _userRepoMock.GetByIdAsync(user.Id, Arg.Any<CancellationToken>()).Returns(user);
        _userRepoMock.TelegramChatIdExistsAsync(chatId, Arg.Any<CancellationToken>()).Returns(false);

        var command = new TelegramAccountLinkedCommand
        {
            UserId = user.Id,
            TelegramChatId = chatId
        };

        // Act
        Result result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        user.TelegramChatId.Should().Be(chatId);
        user.TgEnable.Should().BeTrue();
    }

    [Fact]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "<Pending>")]
    public async Task Handle_WhenUserNotFound_ReturnsNotFoundFailure()
    {
        // Arrange
        var userId = Guid.NewGuid();
        _userRepoMock.GetByIdAsync(userId, Arg.Any<CancellationToken>()).Returns((User?)null);

        var command = new TelegramAccountLinkedCommand
        {
            UserId = userId,
            TelegramChatId = 999L
        };

        // Act
        Result result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Type.Should().Be(ErrorType.NotFound);

        await _userRepoMock.DidNotReceive()
            .TelegramChatIdExistsAsync(Arg.Any<long>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "<Pending>")]
    public async Task Handle_WhenChatIdAlreadyLinkedToAnotherUser_ReturnsConflictFailure()
    {
        // Arrange
        User user = new UserBuilder().WithTgEnable(false).Build();
        long chatId = 777888999L;

        _userRepoMock.GetByIdAsync(user.Id, Arg.Any<CancellationToken>()).Returns(user);
        _userRepoMock.TelegramChatIdExistsAsync(chatId, Arg.Any<CancellationToken>()).Returns(true);

        var command = new TelegramAccountLinkedCommand
        {
            UserId = user.Id,
            TelegramChatId = chatId
        };

        // Act
        Result result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Telegram.AlreadyLinked");
        result.Error.Type.Should().Be(ErrorType.Conflict);
        user.TelegramChatId.Should().BeNull();
    }

    [Fact]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "<Pending>")]
    public async Task Handle_WhenEventIsDeliveredAgainWithSameChatIdForSameUser_ReturnsSuccessWithoutCallingExists()
    {
        // Arrange — simulate idempotent replay: user already has this ChatId
        long chatId = 444555666L;
        User user = new UserBuilder().WithTgEnable(true, chatId).Build();

        _userRepoMock.GetByIdAsync(user.Id, Arg.Any<CancellationToken>()).Returns(user);

        var command = new TelegramAccountLinkedCommand
        {
            UserId = user.Id,
            TelegramChatId = chatId
        };

        // Act
        Result result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();

        // Should NOT call TelegramChatIdExistsAsync — idempotency check short-circuits earlier
        await _userRepoMock.DidNotReceive()
            .TelegramChatIdExistsAsync(Arg.Any<long>(), Arg.Any<CancellationToken>());
    }
}
