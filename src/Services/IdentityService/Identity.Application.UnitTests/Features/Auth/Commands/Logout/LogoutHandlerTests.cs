using IdentityService.Application.Features.Auth.Commands.Logout;

namespace Identity.Application.UnitTests.Features.Auth.Commands.Logout;

public class LogoutHandlerTests
{
    private readonly IRefreshTokenRepository _tokenRepoMock = Substitute.For<IRefreshTokenRepository>();
    private readonly IUserContext _userContextMock = Substitute.For<IUserContext>();
    private readonly LogoutHandler _handler;

    public LogoutHandlerTests()
    {
        _handler = new LogoutHandler(_tokenRepoMock, _userContextMock);
    }

    [Fact]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "<Pending>")]
    public async Task Handle_WithValidUserContext_DeletesTokensByUserIdAndReturnsSuccess()
    {
        // Arrange
        var userId = Guid.NewGuid();
        _userContextMock.UserId.Returns(userId);
        var command = new LogoutCommand();

        // Act
        Result result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        await _tokenRepoMock.Received(1).DeleteTokensByUserIdAsync(userId, Arg.Any<CancellationToken>());
    }
}
