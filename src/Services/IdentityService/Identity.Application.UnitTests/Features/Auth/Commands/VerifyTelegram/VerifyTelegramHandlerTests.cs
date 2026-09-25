using BuildingBlocks.Domain.Results;
using IdentityService.Application.Features.Auth.Commands.VerifyTelegram;
using ZiggyCreatures.Caching.Fusion;

namespace Identity.Application.UnitTests.Features.Auth.Commands.VerifyTelegram;

public class VerifyTelegramHandlerTests
{
    private readonly IFusionCache _cacheMock = Substitute.For<IFusionCache>();
    private readonly IUserRepository _userRepoMock = Substitute.For<IUserRepository>();
    private readonly VerifyTelegramHandler _handler;

    public VerifyTelegramHandlerTests()
    {
        _handler = new VerifyTelegramHandler(_cacheMock, _userRepoMock);
    }

    [Fact]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "<Pending>")]
    public async Task Handle_WithValidTokenAndAvailableChatId_LinksAccountAndReturnsSuccess()
    {
        // Arrange
        User user = new UserBuilder().Build();
        string token = Guid.NewGuid().ToString("N");
        long chatId = 123456789L;
        string cacheKey = $"tg-link:{token}";

        _cacheMock.GetOrDefaultAsync<string>(cacheKey, token: Arg.Any<CancellationToken>())
            .Returns(user.Id.ToString());

        _userRepoMock.TelegramChatIdExistsAsync(chatId, Arg.Any<CancellationToken>())
            .Returns(false);

        _userRepoMock.GetByIdAsync(user.Id, Arg.Any<CancellationToken>())
            .Returns(user);

        var command = new VerifyTelegramCommand { Token = token, ChatId = chatId };

        // Act
        Result result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        user.TelegramChatId.Should().Be(chatId);
    }

    [Fact]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "<Pending>")]
    public async Task Handle_WithValidTokenAndAvailableChatId_RemovesTokenFromCache()
    {
        // Arrange
        User user = new UserBuilder().Build();
        string token = Guid.NewGuid().ToString("N");
        string cacheKey = $"tg-link:{token}";

        _cacheMock.GetOrDefaultAsync<string>(cacheKey, token: Arg.Any<CancellationToken>())
            .Returns(user.Id.ToString());

        _userRepoMock.TelegramChatIdExistsAsync(Arg.Any<long>(), Arg.Any<CancellationToken>())
            .Returns(false);

        _userRepoMock.GetByIdAsync(user.Id, Arg.Any<CancellationToken>())
            .Returns(user);

        var command = new VerifyTelegramCommand { Token = token, ChatId = 111222333L };

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        await _cacheMock.Received(1).RemoveAsync(cacheKey, token: Arg.Any<CancellationToken>());
    }

    [Fact]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "<Pending>")]
    public async Task Handle_WhenTokenNotInCache_ReturnsNotFoundFailure()
    {
        // Arrange
        string token = Guid.NewGuid().ToString("N");
        string cacheKey = $"tg-link:{token}";

        _cacheMock.GetOrDefaultAsync<string>(cacheKey, token: Arg.Any<CancellationToken>())
            .Returns((string?)null);

        var command = new VerifyTelegramCommand { Token = token, ChatId = 111 };

        // Act
        Result result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("TelegramLink.InvalidOrExpired");
        result.Error.Type.Should().Be(ErrorType.NotFound);
    }

    [Fact]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "<Pending>")]
    public async Task Handle_WhenTokenIsNotValidGuid_ReturnsNotFoundFailure()
    {
        // Arrange
        string token = "not-a-valid-token";
        string cacheKey = $"tg-link:{token}";

        _cacheMock.GetOrDefaultAsync<string>(cacheKey, token: Arg.Any<CancellationToken>())
            .Returns("this-is-not-a-guid");

        var command = new VerifyTelegramCommand { Token = token, ChatId = 222 };

        // Act
        Result result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("TelegramLink.InvalidOrExpired");
    }

    [Fact]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "<Pending>")]
    public async Task Handle_WhenChatIdAlreadyLinkedToAnotherUser_ReturnsConflictFailure()
    {
        // Arrange
        User user = new UserBuilder().Build();
        string token = Guid.NewGuid().ToString("N");
        string cacheKey = $"tg-link:{token}";
        long chatId = 999888777L;

        _cacheMock.GetOrDefaultAsync<string>(cacheKey, token: Arg.Any<CancellationToken>())
            .Returns(user.Id.ToString());

        _userRepoMock.TelegramChatIdExistsAsync(chatId, Arg.Any<CancellationToken>())
            .Returns(true);

        var command = new VerifyTelegramCommand { Token = token, ChatId = chatId };

        // Act
        Result result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Telegram.AlreadyLinked");
        result.Error.Type.Should().Be(ErrorType.Conflict);
    }

    [Fact]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "<Pending>")]
    public async Task Handle_WhenUserNotFoundByIdFromCache_ReturnsNotFoundFailure()
    {
        // Arrange
        var userId = Guid.NewGuid();
        string token = Guid.NewGuid().ToString("N");
        string cacheKey = $"tg-link:{token}";

        _cacheMock.GetOrDefaultAsync<string>(cacheKey, token: Arg.Any<CancellationToken>())
            .Returns(userId.ToString());

        _userRepoMock.TelegramChatIdExistsAsync(Arg.Any<long>(), Arg.Any<CancellationToken>())
            .Returns(false);

        _userRepoMock.GetByIdAsync(userId, Arg.Any<CancellationToken>())
            .Returns((User?)null);

        var command = new VerifyTelegramCommand { Token = token, ChatId = 444555666L };

        // Act
        Result result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Type.Should().Be(ErrorType.NotFound);

        // Cache token must NOT be removed when user is not found
        await _cacheMock.DidNotReceive().RemoveAsync(cacheKey, token: Arg.Any<CancellationToken>());
    }
}
