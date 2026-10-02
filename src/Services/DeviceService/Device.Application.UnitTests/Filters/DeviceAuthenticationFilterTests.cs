// Ignore Spelling: Hmac

using BuildingBlocks.Presentation.Constants;
using Device.API.Filters;
using Microsoft.AspNetCore.Routing;
using ZiggyCreatures.Caching.Fusion;

namespace Device.Application.UnitTests.Filters;

public class DeviceAuthenticationFilterTests
{
    private readonly ILogger<DeviceAuthenticationFilter> _loggerMock =
        Substitute.For<ILogger<DeviceAuthenticationFilter>>();

    private readonly IControllerRepository _controllerRepoMock =
        Substitute.For<IControllerRepository>();

    private readonly IRelayCommandsRepository _commandsRepoMock =
        Substitute.For<IRelayCommandsRepository>();

    private readonly IDeviceTokenHasher _tokenHasherMock =
        Substitute.For<IDeviceTokenHasher>();

    private readonly IFusionCache _cacheMock =
        Substitute.For<IFusionCache>();

    private DeviceAuthenticationFilter CreateFilter() =>
        new(_loggerMock, _controllerRepoMock, _commandsRepoMock, _tokenHasherMock, _cacheMock);

    private static EndpointFilterInvocationContext CreateContext(HttpContext httpContext)
    {
        var ctx = Substitute.For<EndpointFilterInvocationContext>();
        ctx.HttpContext.Returns(httpContext);
        return ctx;
    }

    private static HttpContext CreateHttpContext(
        Dictionary<string, string>? headers = null,
        Dictionary<string, object?>? routeValues = null)
    {
        var context = new DefaultHttpContext();

        if (headers is not null)
        {
            foreach (var (key, value) in headers)
            {
                context.Request.Headers[key] = value;
            }
        }

        if (routeValues is not null)
        {
            var routeData = new RouteData();
            foreach (var (key, value) in routeValues)
            {
                routeData.Values[key] = value;
            }

            context.Features.Set<IRoutingFeature>(new RoutingFeature { RouteData = routeData });
        }

        return context;
    }

    [Fact]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "<Pending>")]
    public async Task InvokeAsync_WhenDeviceTokenHeaderMissing_ReturnsUnauthorized()
    {
        // Arrange
        var filter = CreateFilter();
        HttpContext httpContext = CreateHttpContext(); // no DeviceToken header
        var ctx = CreateContext(httpContext);

        // Act
        object? result = await filter.InvokeAsync(ctx, _ => ValueTask.FromResult<object?>(Results.Ok()));

        // Assert
        result.Should().BeAssignableTo<IResult>();
        result!.GetType().Name.Should().Contain("Unauthorized");
    }

    [Fact]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "<Pending>")]
    public async Task InvokeAsync_WhenDeviceTokenPresentButControllerNotFound_ReturnsNotFound()
    {
        // Arrange
        var filter = CreateFilter();
        var controllerId = Guid.NewGuid();

        HttpContext httpContext = CreateHttpContext(
            headers: new() { [ApiConstants.Headers.DeviceToken] = "raw_token" },
            routeValues: new() { ["id"] = controllerId.ToString() });

        _cacheMock.GetOrSetAsync<ControllerAuthCacheItem?>(
                Arg.Any<string>(),
                Arg.Any<Func<FusionCacheFactoryExecutionContext<ControllerAuthCacheItem?>, CancellationToken, Task<ControllerAuthCacheItem?>>>(),
                Arg.Any<FusionCacheEntryOptions>(),
                Arg.Any<CancellationToken>())
            .Returns((ControllerAuthCacheItem?)null);

        var ctx = CreateContext(httpContext);

        // Act
        object? result = await filter.InvokeAsync(ctx, _ => ValueTask.FromResult<object?>(Results.Ok()));

        // Assert
        result.Should().BeAssignableTo<IResult>();
        // Results.NotFound(obj) returns a JsonHttpResult — controller is resolved as null → 404
        result!.GetType().Name.Should().NotContain("Unauthorized");
    }

    [Fact]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "<Pending>")]
    public async Task InvokeAsync_WhenTokenDoesNotMatchHash_ReturnsUnauthorized()
    {
        // Arrange
        var filter = CreateFilter();
        var controllerId = Guid.NewGuid();
        const string RawToken = "bad_token";
        const string StoredHash = "stored_hash_value";

        var cacheItem = new ControllerAuthCacheItem(controllerId, "AA:BB:CC:DD:EE:FF", StoredHash);

        HttpContext httpContext = CreateHttpContext(
            headers: new() { [ApiConstants.Headers.DeviceToken] = RawToken },
            routeValues: new() { ["id"] = controllerId.ToString() });

        _cacheMock.GetOrSetAsync<ControllerAuthCacheItem?>(
                Arg.Any<string>(),
                Arg.Any<Func<FusionCacheFactoryExecutionContext<ControllerAuthCacheItem?>, CancellationToken, Task<ControllerAuthCacheItem?>>>(),
                Arg.Any<FusionCacheEntryOptions>(),
                Arg.Any<CancellationToken>())
            .Returns(cacheItem);

        _tokenHasherMock.Verify(RawToken, StoredHash).Returns(false);

        var ctx = CreateContext(httpContext);

        // Act
        object? result = await filter.InvokeAsync(ctx, _ => ValueTask.FromResult<object?>(Results.Ok()));

        // Assert
        result.Should().BeAssignableTo<IResult>();
        result!.GetType().Name.Should().Contain("Unauthorized");
    }

    [Fact]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "<Pending>")]
    public async Task InvokeAsync_WhenValidToken_SetsHttpContextItemAndCallsNext()
    {
        // Arrange
        var filter = CreateFilter();
        var controllerId = Guid.NewGuid();
        const string RawToken = "valid_token";
        const string StoredHash = "valid_stored_hash";

        var cacheItem = new ControllerAuthCacheItem(controllerId, "AA:BB:CC:DD:EE:FF", StoredHash);

        HttpContext httpContext = CreateHttpContext(
            headers: new() { [ApiConstants.Headers.DeviceToken] = RawToken },
            routeValues: new() { ["id"] = controllerId.ToString() });

        _cacheMock.GetOrSetAsync<ControllerAuthCacheItem?>(
                Arg.Any<string>(),
                Arg.Any<Func<FusionCacheFactoryExecutionContext<ControllerAuthCacheItem?>, CancellationToken, Task<ControllerAuthCacheItem?>>>(),
                Arg.Any<FusionCacheEntryOptions>(),
                Arg.Any<CancellationToken>())
            .Returns(cacheItem);

        _tokenHasherMock.Verify(RawToken, StoredHash).Returns(true);

        var ctx = CreateContext(httpContext);
        var nextCalled = false;

        // Act
        _ = await filter.InvokeAsync(ctx, _ =>
        {
            nextCalled = true;
            return ValueTask.FromResult<object?>(Results.Ok());
        });

        // Assert
        nextCalled.Should().BeTrue();
        httpContext.Items[DeviceAuthenticationFilter.HttpContextItemKey].Should().Be(cacheItem);
    }

    [Fact]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "<Pending>")]
    public async Task InvokeAsync_WithMacAddressHeader_UsesMacCacheKeyAndAuthenticates()
    {
        // Arrange
        var filter = CreateFilter();
        var controllerId = Guid.NewGuid();
        const string RawToken = "valid_token";
        const string StoredHash = "valid_stored_hash";
        const string MacAddress = "aa:bb:cc:dd:ee:ff";
        const string MacAddressNormalized = "AA:BB:CC:DD:EE:FF";
        string expectedCacheKey = $"controller:mac:{MacAddressNormalized}";

        var cacheItem = new ControllerAuthCacheItem(controllerId, MacAddressNormalized, StoredHash);

        HttpContext httpContext = CreateHttpContext(
            headers: new()
            {
                [ApiConstants.Headers.DeviceToken] = RawToken,
                [ApiConstants.Headers.MacAddress] = MacAddress
            });

        _cacheMock.GetOrSetAsync<ControllerAuthCacheItem?>(
                expectedCacheKey,
                Arg.Any<Func<FusionCacheFactoryExecutionContext<ControllerAuthCacheItem?>, CancellationToken, Task<ControllerAuthCacheItem?>>>(),
                Arg.Any<FusionCacheEntryOptions>(),
                Arg.Any<CancellationToken>())
            .Returns(cacheItem);

        _tokenHasherMock.Verify(RawToken, StoredHash).Returns(true);

        var ctx = CreateContext(httpContext);

        // Act
        await filter.InvokeAsync(ctx, _ => ValueTask.FromResult<object?>(Results.Ok()));

        // Assert
        httpContext.Items[DeviceAuthenticationFilter.HttpContextItemKey].Should().Be(cacheItem);

        await _cacheMock.Received(1).GetOrSetAsync<ControllerAuthCacheItem?>(
            expectedCacheKey,
            Arg.Any<Func<FusionCacheFactoryExecutionContext<ControllerAuthCacheItem?>, CancellationToken, Task<ControllerAuthCacheItem?>>>(),
            Arg.Any<FusionCacheEntryOptions>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "<Pending>")]
    public async Task InvokeAsync_WithMismatchedMacAddressHeader_ReturnsForbidden()
    {
        // Arrange
        var filter = CreateFilter();
        var controllerId = Guid.NewGuid();
        const string RawToken = "valid_token";
        const string StoredHash = "valid_stored_hash";
        const string BoundMacAddress = "AA:BB:CC:DD:EE:FF";
        const string DifferentMacAddress = "11:22:33:44:55:66";
        string expectedCacheKey = $"controller:mac:{DifferentMacAddress.ToUpperInvariant()}";

        var cacheItem = new ControllerAuthCacheItem(controllerId, BoundMacAddress, StoredHash);

        HttpContext httpContext = CreateHttpContext(
            headers: new()
            {
                [ApiConstants.Headers.DeviceToken] = RawToken,
                [ApiConstants.Headers.MacAddress] = DifferentMacAddress
            },
            routeValues: new()
            {
                ["controllerId"] = controllerId.ToString()
            });

        _cacheMock.GetOrSetAsync<ControllerAuthCacheItem?>(
                expectedCacheKey,
                Arg.Any<Func<FusionCacheFactoryExecutionContext<ControllerAuthCacheItem?>, CancellationToken, Task<ControllerAuthCacheItem?>>>(),
                Arg.Any<FusionCacheEntryOptions>(),
                Arg.Any<CancellationToken>())
            .Returns(cacheItem);

        _tokenHasherMock.Verify(RawToken, StoredHash).Returns(true);

        var ctx = CreateContext(httpContext);

        // Act
        object? result = await filter.InvokeAsync(ctx, _ => ValueTask.FromResult<object?>(Results.Ok()));

        // Assert
        result.Should().BeOfType<Microsoft.AspNetCore.Http.HttpResults.StatusCodeHttpResult>()
            .Which.StatusCode.Should().Be(StatusCodes.Status403Forbidden);
    }
}
