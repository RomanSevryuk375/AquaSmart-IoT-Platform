using System.Text.Json;
using BuildingBlocks.Domain.Exceptions;
using BuildingBlocks.Presentation.Middlewares;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace BuildingBlocks.UnitTests.Presentation.Middlewares;

public class GlobalExceptionHandlerTests
{
    private readonly GlobalExceptionHandler _handler =
        new(Substitute.For<ILogger<GlobalExceptionHandler>>());

    public static IEnumerable<object[]> ExceptionCases()
    {
        yield return [new ConcurrencyException("conflict"), StatusCodes.Status409Conflict, "Conflict"];
        yield return [new DbUpdateException("db conflict"), StatusCodes.Status409Conflict, "Conflict"];
        yield return [new KeyNotFoundException("missing"), StatusCodes.Status404NotFound, "Not Found"];
        yield return [new ArgumentException("bad arg"), StatusCodes.Status400BadRequest, "Bad Request"];
        yield return [new UnauthorizedAccessException("denied"), StatusCodes.Status401Unauthorized, "Unauthorized"];
        yield return [new InvalidOperationException("boom"), StatusCodes.Status500InternalServerError, "Server Error"];
    }

    [Theory]
    [MemberData(nameof(ExceptionCases))]
    public async Task TryHandleAsync_MapsExceptionToExpectedStatusCodeAndProblemDetails(
        Exception exception,
        int expectedStatusCode,
        string expectedTitle)
    {
        // Arrange
        var httpContext = new DefaultHttpContext();
        httpContext.Response.Body = new MemoryStream();

        // Act
        bool handled = await _handler.TryHandleAsync(httpContext, exception, CancellationToken.None);

        // Assert
        handled.Should().BeTrue();
        httpContext.Response.StatusCode.Should().Be(expectedStatusCode);

        httpContext.Response.Body.Position = 0;
        ProblemDetails? problem = await JsonSerializer.DeserializeAsync<ProblemDetails>(httpContext.Response.Body);
        problem.Should().NotBeNull();
        problem!.Status.Should().Be(expectedStatusCode);
        problem.Title.Should().Be(expectedTitle);
    }
}
