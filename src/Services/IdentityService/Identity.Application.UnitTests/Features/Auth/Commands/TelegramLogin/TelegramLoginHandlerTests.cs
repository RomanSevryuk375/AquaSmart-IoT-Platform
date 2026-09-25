using BuildingBlocks.Domain.Results;
using BuildingBlocks.Presentation.Authorization;
using IdentityService.Application.DTOs;
using IdentityService.Application.Features.Auth.Commands.TelegramLogin;
using Microsoft.Extensions.Options;

namespace Identity.Application.UnitTests.Features.Auth.Commands.TelegramLogin;

public class TelegramLoginHandlerTests
{
    private readonly IUserRepository _userRepoMock = Substitute.For<IUserRepository>();
    private readonly ISubscriptionRepository _subscriptionRepoMock = Substitute.For<ISubscriptionRepository>();
    private readonly IJwtProvider _jwtProviderMock = Substitute.For<IJwtProvider>();
    private readonly TelegramLoginHandler _handler;

    public TelegramLoginHandlerTests()
    {
        var jwtOptions = Options.Create(new JwtOptions
        {
            SecretKey = "test-secret-key",
            Issuer = "Test",
            Audience = "Test",
            ExpiresHours = 24
        });

        _handler = new TelegramLoginHandler(
            _userRepoMock,
            _subscriptionRepoMock,
            _jwtProviderMock,
            jwtOptions);
    }

    [Fact]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "<Pending>")]
    public async Task Handle_WhenUserLinkedToTelegramChatId_ReturnsAccessToken()
    {
        // Arrange
        User user = new UserBuilder().Build();
        long chatId = 123456789L;
        user.LinkTelegramChat(chatId);

        Subscription subscription = new SubscriptionBuilder().Build();

        _userRepoMock.GetByTelegramChatIdAsync(chatId, Arg.Any<CancellationToken>())
            .Returns(user);

        _subscriptionRepoMock.GetByIdAsync(user.SubscriptionId, Arg.Any<CancellationToken>())
            .Returns(subscription);

        _jwtProviderMock.GenerateToken(user, Arg.Any<List<string>>())
            .Returns("telegram_jwt_token");

        var command = new TelegramLoginCommand { ChatId = chatId };

        // Act
        Result<TelegramLoginResponseDto> result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.AccessToken.Should().Be("telegram_jwt_token");
        result.Value.ExpiresHours.Should().Be(24);
    }

    [Fact]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "<Pending>")]
    public async Task Handle_WhenUserNotFoundByChatId_ReturnsNotFoundFailure()
    {
        // Arrange
        long chatId = 999000111L;

        _userRepoMock.GetByTelegramChatIdAsync(chatId, Arg.Any<CancellationToken>())
            .Returns((User?)null);

        var command = new TelegramLoginCommand { ChatId = chatId };

        // Act
        Result<TelegramLoginResponseDto> result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Type.Should().Be(ErrorType.NotFound);

        _jwtProviderMock.DidNotReceive().GenerateToken(Arg.Any<User>(), Arg.Any<List<string>>());
    }

    [Fact]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "<Pending>")]
    public async Task Handle_WhenSubscriptionIsNull_ReturnsSuccessWithEmptyPermissions()
    {
        // Arrange
        User user = new UserBuilder().Build();
        long chatId = 555111222L;
        user.LinkTelegramChat(chatId);

        _userRepoMock.GetByTelegramChatIdAsync(chatId, Arg.Any<CancellationToken>())
            .Returns(user);

        _subscriptionRepoMock.GetByIdAsync(user.SubscriptionId, Arg.Any<CancellationToken>())
            .Returns((Subscription?)null);

        _jwtProviderMock.GenerateToken(user, Arg.Is<List<string>>(p => p.Count == 0))
            .Returns("token_no_permissions");

        var command = new TelegramLoginCommand { ChatId = chatId };

        // Act
        Result<TelegramLoginResponseDto> result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.AccessToken.Should().Be("token_no_permissions");

        _jwtProviderMock.Received(1).GenerateToken(
            user,
            Arg.Is<List<string>>(p => p.Count == 0));
    }
}
