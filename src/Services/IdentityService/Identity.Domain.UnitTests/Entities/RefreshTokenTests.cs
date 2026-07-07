using Contracts.Results;
using FluentAssertions;
using Identity.TestShared.Builders;
using IdentityService.Domain.Entities;

namespace Identity.Domain.UnitTests.Entities;

public class RefreshTokenTests
{
    [Fact]
    public void Create_WithEmptyUserId_ReturnsFailure()
    {
        // Act
        Result<RefreshToken> result = RefreshToken.Create(Guid.Empty, "some-hash");

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("RefreshToken.Invalid");
        result.Error.Message.Should().Be("UserId cannot be empty.");
        result.Error.Type.Should().Be(ErrorType.Validation);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithEmptyTokenHash_ReturnsFailure(string? invalidHash)
    {
        // Act
        Result<RefreshToken> result = RefreshToken.Create(Guid.NewGuid(), invalidHash!);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("RefreshToken.Invalid");
        result.Error.Message.Should().Be("Token hash cannot be empty.");
        result.Error.Type.Should().Be(ErrorType.Validation);
    }

    [Fact]
    public void Create_WithValidData_ReturnsSuccessAndInitializesFields()
    {
        // Arrange
        var userId = Guid.NewGuid();
        string tokenHash = "token-hash-12345";
        var tolerance = TimeSpan.FromSeconds(5);

        // Act
        Result<RefreshToken> result = RefreshToken.Create(userId, tokenHash);

        // Assert
        result.IsSuccess.Should().BeTrue();
        RefreshToken token = result.Value;
        token.Id.Should().NotBeEmpty();
        token.UserId.Should().Be(userId);
        token.TokenHash.Should().Be(tokenHash);
        token.IsUsed.Should().BeFalse();
        token.IsRevoked.Should().BeFalse();
        token.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, tolerance);
        token.ExpiredAt.Should().BeCloseTo(DateTime.UtcNow.AddDays(30), tolerance);
        token.Version.Should().BeEmpty();
    }

    [Fact]
    public void MarkAsUsed_WhenCalled_SetsIsUsedToTrueAndIncrementsVersion()
    {
        // Arrange
        RefreshToken token = new RefreshTokenBuilder().Build();
        Guid initialVersion = token.Version;

        // Act
        token.MarkAsUsed();

        // Assert
        token.IsUsed.Should().BeTrue();
        token.Version.Should().NotBe(initialVersion);
    }

    [Fact]
    public void MarkAsRevoked_WhenCalled_SetsIsRevokedToTrueAndIncrementsVersion()
    {
        // Arrange
        RefreshToken token = new RefreshTokenBuilder().Build();
        Guid initialVersion = token.Version;

        // Act
        token.MarkAsRevoked();

        // Assert
        token.IsRevoked.Should().BeTrue();
        token.Version.Should().NotBe(initialVersion);
    }
}
