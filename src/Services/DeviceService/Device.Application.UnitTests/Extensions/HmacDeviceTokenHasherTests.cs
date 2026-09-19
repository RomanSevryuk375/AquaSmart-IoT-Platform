using Device.Application.Extesions;
using Microsoft.Extensions.Options;

namespace Device.Application.UnitTests.Extensions;

public class HmacDeviceTokenHasherTests
{
    private const string TestSecret = "test-super-secret-key-that-is-at-least-32-chars-long";
    private readonly HmacDeviceTokenHasher _hasher;

    public HmacDeviceTokenHasherTests()
    {
        IOptions<DeviceSettings> options = Options.Create(new DeviceSettings
        {
            TokenHmacSecret = TestSecret
        });

        _hasher = new HmacDeviceTokenHasher(options);
    }

    [Fact]
    public void GenerateRawToken_ShouldReturnTokenWithPrefixAndValidLength()
    {
        // Act
        string token = _hasher.GenerateRawToken();

        // Assert
        token.Should().NotBeNullOrWhiteSpace();
        token.Should().StartWith("ak_");
        token.Length.Should().BeGreaterThan(35);
        token.Should().NotContain("+");
        token.Should().NotContain("/");
        token.Should().NotContain("=");
    }

    [Fact]
    public void GenerateRawToken_ShouldReturnUniqueTokens()
    {
        // Act
        string token1 = _hasher.GenerateRawToken();
        string token2 = _hasher.GenerateRawToken();

        // Assert
        token1.Should().NotBe(token2);
    }

    [Fact]
    public void ComputeHash_WithValidToken_ShouldReturnConsistentHmac()
    {
        // Arrange
        string token = _hasher.GenerateRawToken();

        // Act
        string hash1 = _hasher.ComputeHash(token);
        string hash2 = _hasher.ComputeHash(token);

        // Assert
        hash1.Should().NotBeNullOrWhiteSpace();
        hash1.Should().Be(hash2);
    }

    [Fact]
    public void ComputeHash_WithDifferentTokens_ShouldReturnDifferentHashes()
    {
        // Arrange
        string token1 = _hasher.GenerateRawToken();
        string token2 = _hasher.GenerateRawToken();

        // Act
        string hash1 = _hasher.ComputeHash(token1);
        string hash2 = _hasher.ComputeHash(token2);

        // Assert
        hash1.Should().NotBe(hash2);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void ComputeHash_WithNullOrWhiteSpace_ShouldThrowArgumentException(string? invalidToken)
    {
        // Act
        Action act = () => _hasher.ComputeHash(invalidToken!);

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Verify_WithValidHmacTokenAndHash_ShouldReturnTrue()
    {
        // Arrange
        string token = _hasher.GenerateRawToken();
        string hash = _hasher.ComputeHash(token);

        // Act
        bool isValid = _hasher.Verify(token, hash);

        // Assert
        isValid.Should().BeTrue();
    }

    [Fact]
    public void Verify_WithInvalidTokenAndHmacHash_ShouldReturnFalse()
    {
        // Arrange
        string token = _hasher.GenerateRawToken();
        string hash = _hasher.ComputeHash(token);

        // Act
        bool isValid = _hasher.Verify("ak_different_token", hash);

        // Assert
        isValid.Should().BeFalse();
    }

    [Fact]
    public void Verify_WithLegacyBcryptHash_ShouldVerifySuccessfully()
    {
        // Arrange
        const string rawToken = "legacy_token_123";
        string bcryptHash = BCrypt.Net.BCrypt.EnhancedHashPassword(rawToken);

        // Act
        bool isValid = _hasher.Verify(rawToken, bcryptHash);

        // Assert
        isValid.Should().BeTrue();
    }

    [Fact]
    public void Verify_WithLegacyBcryptHashAndInvalidToken_ShouldReturnFalse()
    {
        // Arrange
        const string rawToken = "legacy_token_123";
        string bcryptHash = BCrypt.Net.BCrypt.EnhancedHashPassword(rawToken);

        // Act
        bool isValid = _hasher.Verify("wrong_token", bcryptHash);

        // Assert
        isValid.Should().BeFalse();
    }

    [Theory]
    [InlineData(null, "some_hash")]
    [InlineData("", "some_hash")]
    [InlineData("token", null)]
    [InlineData("token", "")]
    [InlineData("  ", "  ")]
    public void Verify_WithNullOrWhiteSpace_ShouldReturnFalse(string? rawToken, string? storedHash)
    {
        // Act
        bool isValid = _hasher.Verify(rawToken!, storedHash!);

        // Assert
        isValid.Should().BeFalse();
    }

    [Fact]
    public void Constructor_WithEmptySecret_ShouldUseFallbackSecretAndWork()
    {
        // Arrange
        IOptions<DeviceSettings> emptyOptions = Options.Create(new DeviceSettings
        {
            TokenHmacSecret = ""
        });
        var hasherWithDefaultSecret = new HmacDeviceTokenHasher(emptyOptions);

        string token = hasherWithDefaultSecret.GenerateRawToken();
        string hash = hasherWithDefaultSecret.ComputeHash(token);

        // Act
        bool isValid = hasherWithDefaultSecret.Verify(token, hash);

        // Assert
        isValid.Should().BeTrue();
    }
}
