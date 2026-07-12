using Microsoft.Extensions.Caching.Memory;
using Telemetry.Application.DTOs;
using Telemetry.Application.Interfaces;
using Telemetry.Infrastructure.GrpcClients;

namespace Telemetry.Application.UnitTests.Features.Telemetry;

public class CachedDeviceTokenValidatorTests
{
    [Fact]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "<Pending>")]
    public async Task ValidateAsync_OnCacheMissAndSuccess_CallsInnerValidatorAndReturnsSuccess()
    {
        // Arrange
        string macAddress = "00:1A:2B:3C:4D:5E";
        string deviceToken = "some-token";
        var expectedResponse = new ValidateResponseDto
        {
            ControllerId = Guid.NewGuid(),
            UserId = Guid.NewGuid()
        };

        IDeviceTokenValidator innerValidatorMock = Substitute.For<IDeviceTokenValidator>();
        innerValidatorMock.ValidateAsync(macAddress, deviceToken, Arg.Any<CancellationToken>())
            .Returns(Result<ValidateResponseDto>.Success(expectedResponse));

        IMemoryCache realCache = new MemoryCache(new MemoryCacheOptions());
        var cachedValidator = new CachedDeviceTokenValidator(innerValidatorMock, realCache);

        // Act
        Result<ValidateResponseDto> result = await cachedValidator.ValidateAsync(macAddress, deviceToken, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEquivalentTo(expectedResponse);
        await innerValidatorMock.Received(1).ValidateAsync(macAddress, deviceToken, Arg.Any<CancellationToken>());
    }

    [Fact]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "<Pending>")]
    public async Task ValidateAsync_OnCacheHit_CallsInnerValidatorOnlyOnceAndReturnsCachedValue()
    {
        // Arrange
        string macAddress = "00:1A:2B:3C:4D:5E";
        string deviceToken = "some-token";
        var expectedResponse = new ValidateResponseDto
        {
            ControllerId = Guid.NewGuid(),
            UserId = Guid.NewGuid()
        };

        IDeviceTokenValidator innerValidatorMock = Substitute.For<IDeviceTokenValidator>();
        innerValidatorMock.ValidateAsync(macAddress, deviceToken, Arg.Any<CancellationToken>())
            .Returns(Result<ValidateResponseDto>.Success(expectedResponse));

        IMemoryCache realCache = new MemoryCache(new MemoryCacheOptions());
        var cachedValidator = new CachedDeviceTokenValidator(innerValidatorMock, realCache);

        // Act
        // First call (Cache Miss)
        Result<ValidateResponseDto> result1 = await cachedValidator.ValidateAsync(macAddress, deviceToken, CancellationToken.None);

        // Second call (Cache Hit)
        Result<ValidateResponseDto> result2 = await cachedValidator.ValidateAsync(macAddress, deviceToken, CancellationToken.None);

        // Assert
        result1.IsSuccess.Should().BeTrue();
        result2.IsSuccess.Should().BeTrue();
        result2.Value.Should().BeEquivalentTo(expectedResponse);

        // Verify inner validator was called ONLY ONCE
        await innerValidatorMock.Received(1).ValidateAsync(macAddress, deviceToken, Arg.Any<CancellationToken>());
    }
}
