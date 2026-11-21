using BuildingBlocks.Application.Behaviors;
using BuildingBlocks.Domain.Abstractions;
using BuildingBlocks.Domain.Results;
using Microsoft.Extensions.Logging;
using ZiggyCreatures.Caching.Fusion;

namespace BuildingBlocks.UnitTests.Application.Behaviors;

public sealed class CacheBehaviorTests : IDisposable
{
    public sealed record TestCachedQuery(string CacheKey) : ICachedQuery<Result<string>>
    {
        public TimeSpan? Expiration => TimeSpan.FromMinutes(1);
        public bool AllowFailSafe => false;
    }

    private readonly FusionCache _cache = new(new FusionCacheOptions());
    private readonly ILogger<CacheBehavior<TestCachedQuery, Result<string>>> _logger =
        Substitute.For<ILogger<CacheBehavior<TestCachedQuery, Result<string>>>>();

    public void Dispose()
    {
        _cache.Dispose();
        GC.SuppressFinalize(this);
    }

    [Fact]
    public async Task Handle_OnCacheMissAndSuccess_ExecutesNextAndCachesResult()
    {
        // Arrange
        var behavior = new CacheBehavior<TestCachedQuery, Result<string>>(_cache, _logger);
        var query = new TestCachedQuery($"key-{Guid.NewGuid()}");
        int nextCalls = 0;

        RequestHandlerDelegate<Result<string>> next = _ =>
        {
            nextCalls++;
            return Task.FromResult(Result<string>.Success("cached-payload"));
        };

        // Act
        Result<string> first = await behavior.Handle(query, next, CancellationToken.None);
        Result<string> second = await behavior.Handle(query, next, CancellationToken.None);

        // Assert
        first.IsSuccess.Should().BeTrue();
        first.Value.Should().Be("cached-payload");
        second.IsSuccess.Should().BeTrue();
        second.Value.Should().Be("cached-payload");
        nextCalls.Should().Be(1);
    }

    [Fact]
    public async Task Handle_OnFailureResult_DoesNotCacheFailure()
    {
        // Arrange
        var behavior = new CacheBehavior<TestCachedQuery, Result<string>>(_cache, _logger);
        var query = new TestCachedQuery($"fail-key-{Guid.NewGuid()}");
        int nextCalls = 0;

        RequestHandlerDelegate<Result<string>> next = _ =>
        {
            nextCalls++;
            return Task.FromResult(Result<string>.Failure(Error.NotFound("Item.NotFound", "Missing")));
        };

        // Act
        Result<string> first = await behavior.Handle(query, next, CancellationToken.None);
        Result<string> second = await behavior.Handle(query, next, CancellationToken.None);

        // Assert
        first.IsFailure.Should().BeTrue();
        second.IsFailure.Should().BeTrue();
        nextCalls.Should().Be(2);
    }
}
