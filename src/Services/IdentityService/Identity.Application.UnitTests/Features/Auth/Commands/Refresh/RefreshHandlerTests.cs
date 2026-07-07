using IdentityService.Application.DTOs;
using IdentityService.Application.Features.Auth.Commands.Refresh;
using Microsoft.AspNetCore.Identity;

namespace Identity.Application.UnitTests.Features.Auth.Commands.Refresh;

public class RefreshHandlerTests
{
    private readonly UserManager<User> _userManagerMock;
    private readonly ISubscriptionRepository _subscriptionRepoMock = Substitute.For<ISubscriptionRepository>();
    private readonly IRefreshTokenRepository _tokenRepoMock = Substitute.For<IRefreshTokenRepository>();
    private readonly IJwtProvider _jwtProviderMock = Substitute.For<IJwtProvider>();
    private readonly IMyHasher _hasherMock = Substitute.For<IMyHasher>();
    private readonly RefreshHandler _handler;

    public RefreshHandlerTests()
    {
        IUserStore<User> storeMock = Substitute.For<IUserStore<User>>();
        _userManagerMock = Substitute.For<UserManager<User>>(storeMock, null, null, null, null, null, null, null, null);
        _handler = new RefreshHandler(_userManagerMock, _subscriptionRepoMock, _tokenRepoMock, _jwtProviderMock, _hasherMock);
    }

    [Fact]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "<Pending>")]
    public async Task Handle_WithInvalidTokenFormat_ReturnsValidationError()
    {
        // Arrange
        var command = new RefreshCommand { RefreshToken = "invalid-token-format" };

        // Act
        Result<LoginResponseDto> result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("RefreshToken.Invalid");
        result.Error.Message.Should().Be("Invalid token format.");
    }

    [Fact]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "<Pending>")]
    public async Task Handle_WhenTokenNotFound_ReturnsValidationError()
    {
        // Arrange
        var tokenId = Guid.NewGuid();
        var command = new RefreshCommand { RefreshToken = $"{tokenId}.raw_secret" };

        _tokenRepoMock.GetByIdAsync(tokenId, Arg.Any<CancellationToken>()).Returns((RefreshToken?)null);

        // Act
        Result<LoginResponseDto> result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("RefreshToken.Invalid");
        result.Error.Message.Should().Be("Token is invalid or expired.");
    }

    [Fact]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "<Pending>")]
    public async Task Handle_WhenTokenIsUsed_RevokesAllTokensAndReturnsConflictError()
    {
        // Arrange
        var userId = Guid.NewGuid();
        RefreshToken token = new RefreshTokenBuilder().WithUserId(userId).AsUsed().Build();
        var command = new RefreshCommand { RefreshToken = $"{token.Id}.raw_secret" };

        _tokenRepoMock.GetByIdAsync(token.Id, Arg.Any<CancellationToken>()).Returns(token);

        // Act
        Result<LoginResponseDto> result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Security.Breach");
        result.Error.Message.Should().Be("Token reuse detected. All sessions revoked.");

        await _tokenRepoMock.Received(1).DeleteTokensByUserIdAsync(userId, Arg.Any<CancellationToken>());
    }

    [Fact]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "<Pending>")]
    public async Task Handle_WhenTokenIsRevoked_ReturnsValidationError()
    {
        // Arrange
        RefreshToken token = new RefreshTokenBuilder().AsRevoked().Build();
        var command = new RefreshCommand { RefreshToken = $"{token.Id}.raw_secret" };

        _tokenRepoMock.GetByIdAsync(token.Id, Arg.Any<CancellationToken>()).Returns(token);

        // Act
        Result<LoginResponseDto> result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("RefreshToken.Invalid");
        result.Error.Message.Should().Be("Token is invalid or expired.");
    }

    [Fact]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "<Pending>")]
    public async Task Handle_WhenTokenIsExpired_ReturnsValidationError()
    {
        // Arrange
        RefreshToken token = new RefreshTokenBuilder().Build();
        typeof(RefreshToken).GetProperty(nameof(RefreshToken.ExpiredAt))!
            .SetValue(token, DateTime.UtcNow.AddDays(-1));

        var command = new RefreshCommand { RefreshToken = $"{token.Id}.raw_secret" };

        _tokenRepoMock.GetByIdAsync(token.Id, Arg.Any<CancellationToken>()).Returns(token);

        // Act
        Result<LoginResponseDto> result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("RefreshToken.Invalid");
        result.Error.Message.Should().Be("Token is invalid or expired.");
    }

    [Fact]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "<Pending>")]
    public async Task Handle_WhenTokenHashVerificationFails_ReturnsValidationError()
    {
        // Arrange
        RefreshToken token = new RefreshTokenBuilder().WithTokenHash("hashed_secret").Build();
        var command = new RefreshCommand { RefreshToken = $"{token.Id}.wrong_secret" };

        _tokenRepoMock.GetByIdAsync(token.Id, Arg.Any<CancellationToken>()).Returns(token);
        _hasherMock.Verify("wrong_secret", "hashed_secret").Returns(false);

        // Act
        Result<LoginResponseDto> result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("RefreshToken.Invalid");
        result.Error.Message.Should().Be("Token is invalid or expired.");
    }

    [Fact]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "<Pending>")]
    public async Task Handle_WhenUserNotFound_ReturnsNotFoundError()
    {
        // Arrange
        var userId = Guid.NewGuid();
        RefreshToken token = new RefreshTokenBuilder().WithUserId(userId).WithTokenHash("hashed_secret").Build();
        var command = new RefreshCommand { RefreshToken = $"{token.Id}.raw_secret" };

        _tokenRepoMock.GetByIdAsync(token.Id, Arg.Any<CancellationToken>()).Returns(token);
        _hasherMock.Verify("raw_secret", "hashed_secret").Returns(true);
        _userManagerMock.FindByIdAsync(userId.ToString()).Returns((User?)null);

        // Act
        Result<LoginResponseDto> result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("User.NotFound");
        result.Error.Message.Should().Be("User not found.");
    }

    [Fact]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "<Pending>")]
    public async Task Handle_WithValidRequest_ReturnsNewTokensAndMarksOldTokenAsUsed()
    {
        // Arrange
        User user = new UserBuilder().Build();
        Subscription subscription = new SubscriptionBuilder().Build();
        RefreshToken oldToken = new RefreshTokenBuilder()
            .WithUserId(user.Id)
            .WithTokenHash("old_hashed_secret")
            .Build();
        var command = new RefreshCommand { RefreshToken = $"{oldToken.Id}.old_raw_secret" };

        _tokenRepoMock.GetByIdAsync(oldToken.Id, Arg.Any<CancellationToken>()).Returns(oldToken);
        _hasherMock.Verify("old_raw_secret", "old_hashed_secret").Returns(true);
        _userManagerMock.FindByIdAsync(user.Id.ToString()).Returns(user);

        _subscriptionRepoMock.GetByIdAsync(user.SubscriptionId, Arg.Any<CancellationToken>())
            .Returns(subscription);

        _jwtProviderMock.GenerateToken(user, Arg.Any<List<string>>()).Returns("new_access_token");
        _jwtProviderMock.GenerateRefreshToken().Returns("new_raw_secret");
        _hasherMock.Generate("new_raw_secret").Returns("new_hashed_secret");

        // Act
        Result<LoginResponseDto> result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.AccessToken.Should().Be("new_access_token");
        result.Value.RefreshToken.Should().Contain("new_raw_secret");

        oldToken.IsUsed.Should().BeTrue();
        await _tokenRepoMock.Received(1)
            .AddAsync(Arg.Is<RefreshToken>(rt => rt.UserId == user.Id && rt.TokenHash == "new_hashed_secret"),
            Arg.Any<CancellationToken>());
    }
}
