using IdentityService.Application.DTOs;
using IdentityService.Application.Features.Auth.Commands.Register;
using Microsoft.AspNetCore.Identity;

namespace Identity.Application.UnitTests.Features.Auth.Commands.Register;

public class RegisterHandlerTests
{
    private readonly UserManager<User> _userManagerMock;
    private readonly ISubscriptionRepository _subscriptionRepoMock = Substitute.For<ISubscriptionRepository>();
    private readonly IRefreshTokenRepository _tokenRepoMock = Substitute.For<IRefreshTokenRepository>();
    private readonly IJwtProvider _jwtProviderMock = Substitute.For<IJwtProvider>();
    private readonly IMyHasher _hasherMock = Substitute.For<IMyHasher>();
    private readonly RegisterHandler _handler;

    public RegisterHandlerTests()
    {
        IUserStore<User> storeMock = Substitute.For<IUserStore<User>>();
        _userManagerMock = Substitute.For<UserManager<User>>(storeMock, null, null, null, null, null, null, null, null);
        _handler = new RegisterHandler(_userManagerMock, _subscriptionRepoMock, _tokenRepoMock, _jwtProviderMock, _hasherMock);
    }

    [Fact]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "<Pending>")]
    public async Task Handle_WithValidRequest_ReturnsTokensAndPersistsRefreshToken()
    {
        // Arrange
        var command = new RegisterCommand
        {
            Email = "new.user@example.com",
            Name = "New User",
            Password = "Password123!",
            PhoneNumber = "+375291112233",
            TimeZone = "Europe/Minsk"
        };

        Subscription subscription = new SubscriptionBuilder().Build();

        _userManagerMock.FindByEmailAsync(command.Email).Returns((User?)null);
        _userManagerMock.CreateAsync(Arg.Any<User>(), command.Password).Returns(IdentityResult.Success);

        _subscriptionRepoMock.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns(subscription);

        _jwtProviderMock.GenerateToken(Arg.Any<User>(), Arg.Any<List<string>>()).Returns("access_token");
        _jwtProviderMock.GenerateRefreshToken().Returns("raw_refresh_token");
        _hasherMock.Generate("raw_refresh_token").Returns("hashed_token");

        // Act
        Result<LoginResponseDto> result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.AccessToken.Should().Be("access_token");
        result.Value.RefreshToken.Should().Contain("raw_refresh_token");

        await _tokenRepoMock.Received(1)
            .AddAsync(Arg.Is<RefreshToken>(rt => rt.TokenHash == "hashed_token"),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "<Pending>")]
    public async Task Handle_WhenEmailAlreadyExists_ReturnsConflictError()
    {
        // Arrange
        User existingUser = new UserBuilder().Build();
        var command = new RegisterCommand
        {
            Email = existingUser.Email!,
            Name = "New User",
            Password = "Password123!",
            PhoneNumber = "+375291112233",
            TimeZone = "Europe/Minsk"
        };

        _userManagerMock.FindByEmailAsync(command.Email).Returns(existingUser);

        // Act
        Result<LoginResponseDto> result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Email.Busy");
        result.Error.Message.Should().Be("Email is already in use.");

        await _userManagerMock.DidNotReceive().CreateAsync(Arg.Any<User>(), Arg.Any<string>());
    }

    [Fact]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "<Pending>")]
    public async Task Handle_WhenIdentityCreationFails_ReturnsConflictErrorWithErrorsList()
    {
        // Arrange
        var command = new RegisterCommand
        {
            Email = "new.user@example.com",
            Name = "New User",
            Password = "weak",
            PhoneNumber = "+375291112233",
            TimeZone = "Europe/Minsk"
        };

        _userManagerMock.FindByEmailAsync(command.Email).Returns((User?)null);

        var identityError = new IdentityError { Description = "Password requires non-alphanumeric character." };
        _userManagerMock.CreateAsync(Arg.Any<User>(), command.Password)
            .Returns(IdentityResult.Failed(identityError));

        // Act
        Result<LoginResponseDto> result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Register.Failure");
        result.Error.Message.Should().Be("Password requires non-alphanumeric character.");

        await _tokenRepoMock.DidNotReceive().AddAsync(Arg.Any<RefreshToken>(), Arg.Any<CancellationToken>());
    }
}
