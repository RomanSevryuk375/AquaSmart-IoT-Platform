using BuildingBlocks.Domain.Results;
using IdentityService.Application.Features.BackgroundJobs.Commands.CleanIncorrectTokens;

namespace Identity.Application.UnitTests.Features.BackgroundJobs.Commands.CleanIncorrectTokens;

public class CleanIncorrectTokensHandlerTests
{
    private readonly IRefreshTokenRepository _tokenRepoMock = Substitute.For<IRefreshTokenRepository>();
    private readonly CleanIncorrectTokensHandler _handler;

    public CleanIncorrectTokensHandlerTests()
    {
        _handler = new CleanIncorrectTokensHandler(_tokenRepoMock);
    }

    [Fact]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "<Pending>")]
    public async Task Handle_CallsDeleteIncorrectTokensAsync_AndReturnsSuccess()
    {
        // Arrange
        var command = new CleanIncorrectTokensCommand();

        // Act
        Result result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        await _tokenRepoMock.Received(1).DeleteIncorrectTokensAsync(Arg.Any<CancellationToken>());
    }
}
