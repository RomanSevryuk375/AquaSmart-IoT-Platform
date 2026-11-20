using BuildingBlocks.Application.Behaviors;
using BuildingBlocks.Domain.Results;
using Microsoft.Extensions.Logging;

namespace BuildingBlocks.UnitTests.Application.Behaviors;

public class LoggingBehaviorTests
{
    public sealed record SampleRequest : IRequest<Result>;

    [Fact]
    public async Task Handle_CallsNextAndReturnsResponse()
    {
        // Arrange
        ILogger<LoggingBehavior<SampleRequest, Result>> logger =
            Substitute.For<ILogger<LoggingBehavior<SampleRequest, Result>>>();
        var behavior = new LoggingBehavior<SampleRequest, Result>(logger);
        RequestHandlerDelegate<Result> next = Substitute.For<RequestHandlerDelegate<Result>>();
        next.Invoke(Arg.Any<CancellationToken>()).Returns(Result.Success());

        // Act
        Result result = await behavior.Handle(new SampleRequest(), next, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        await next.Received(1).Invoke(Arg.Any<CancellationToken>());
    }
}
