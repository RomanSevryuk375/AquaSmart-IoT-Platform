using System.Security.Claims;
using BuildingBlocks.Presentation.Extensions;

namespace BuildingBlocks.UnitTests.Presentation.Extensions;

public class ClaimsPrincipalExtensionsTests
{
    [Fact]
    public void GetUserId_WithValidNameIdentifierClaim_ReturnsGuid()
    {
        // Arrange
        var expectedId = Guid.NewGuid();
        var principal = new ClaimsPrincipal(new ClaimsIdentity(
            [new Claim(ClaimTypes.NameIdentifier, expectedId.ToString())],
            "TestAuth"));

        // Act
        Guid userId = principal.GetUserId();

        // Assert
        userId.Should().Be(expectedId);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("not-a-guid")]
    public void GetUserId_WithMissingOrInvalidClaim_ReturnsEmptyGuid(string? claimValue)
    {
        // Arrange
        Claim[] claims = claimValue is null
            ? []
            : [new Claim(ClaimTypes.NameIdentifier, claimValue)];
        var principal = new ClaimsPrincipal(new ClaimsIdentity(claims));

        // Act
        Guid userId = principal.GetUserId();

        // Assert
        userId.Should().Be(Guid.Empty);
    }
}
