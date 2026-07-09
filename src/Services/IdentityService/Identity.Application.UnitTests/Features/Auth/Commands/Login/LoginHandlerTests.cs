using IdentityService.Application.DTOs;
using IdentityService.Application.Features.Auth.Commands.Login;
using Microsoft.AspNetCore.Identity;

namespace Identity.Application.UnitTests.Features.Auth.Commands.Login;

public class LoginHandlerTests
{
    private readonly UserManager<User> _userManagerMock;
    private readonly ISubscriptionRepository _subscriptionRepoMock = Substitute.For<ISubscriptionRepository>();
    private readonly IRefreshTokenRepository _tokenRepoMock = Substitute.For<IRefreshTokenRepository>();
    private readonly IJwtProvider _jwtProviderMock = Substitute.For<IJwtProvider>();
    private readonly IMyHasher _hasherMock = Substitute.For<IMyHasher>();
    private readonly LoginHandler _handler;

    public LoginHandlerTests()
    {
        IUserStore<User> storeMock = Substitute.For<IUserStore<User>>();
        _userManagerMock = Substitute.For<UserManager<User>>(storeMock, null, null, null, null, null, null, null, null);
        _handler = new LoginHandler(_userManagerMock, _subscriptionRepoMock, _tokenRepoMock, _jwtProviderMock, _hasherMock);
    }

    [Fact]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "<Pending>")]
    public async Task Handle_WithValidCredentials_ReturnsTokensAndPersistsRefreshToken()
    {
        // Arrange
        User user = new UserBuilder().Build();
        var command = new LoginCommand { Email = user.Email!, Password = "Password123!" };
        Subscription subscription = new SubscriptionBuilder().Build();

        _userManagerMock.FindByEmailAsync(user.Email!).Returns(user);
        _userManagerMock.CheckPasswordAsync(user, command.Password).Returns(true);

        _subscriptionRepoMock.GetByIdAsync(user.SubscriptionId, Arg.Any<CancellationToken>())
            .Returns(subscription);

        _jwtProviderMock.GenerateToken(user, Arg.Any<List<string>>()).Returns("access_token");
        _jwtProviderMock.GenerateRefreshToken().Returns("raw_refresh_token");
        _hasherMock.Generate("raw_refresh_token").Returns("hashed_token");

        // Act
        Result<LoginResponseDto> result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.AccessToken.Should().Be("access_token");
        result.Value.RefreshToken.Should().Contain("raw_refresh_token");

        await _tokenRepoMock.Received(1)
            .AddAsync(Arg.Is<RefreshToken>(rt => rt.UserId == user.Id && rt.TokenHash == "hashed_token"),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "<Pending>")]
    public async Task Handle_WhenUserNotFound_ReturnsAuthInvalidError()
    {
        // Arrange
        var command = new LoginCommand { Email = "nonexistent@example.com", Password = "Password123!" };
        _userManagerMock.FindByEmailAsync(command.Email).Returns((User?)null);

        // Act
        Result<LoginResponseDto> result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Auth.Invalid");
        result.Error.Message.Should().Be("Invalid credentials.");

        await _tokenRepoMock.DidNotReceive().AddAsync(Arg.Any<RefreshToken>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "<Pending>")]
    public async Task Handle_WhenPasswordIsInvalid_ReturnsAuthInvalidError()
    {
        // Arrange
        User user = new UserBuilder().Build();
        var command = new LoginCommand { Email = user.Email!, Password = "WrongPassword" };

        _userManagerMock.FindByEmailAsync(user.Email!).Returns(user);
        _userManagerMock.CheckPasswordAsync(user, command.Password).Returns(false);

        // Act
        Result<LoginResponseDto> result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Auth.Invalid");
        result.Error.Message.Should().Be("Invalid credentials.");

        await _tokenRepoMock.DidNotReceive().AddAsync(Arg.Any<RefreshToken>(), Arg.Any<CancellationToken>());
    }
}
