using BuildingBlocks.Domain.Results;
using BuildingBlocks.Presentation.ResultExtensions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace BuildingBlocks.UnitTests.Presentation.ResultExtensions;

public class ResultExtensionsTests
{
    private sealed class DummyController : ControllerBase;

    [Fact]
    public void ToActionResult_WhenNonGenericSuccess_ReturnsNoContentResult()
    {
        // Arrange
        var controller = new DummyController();

        // Act
        ActionResult actionResult = controller.ToActionResult(Result.Success());

        // Assert
        actionResult.Should().BeOfType<NoContentResult>();
    }

    [Fact]
    public void ToActionResult_WhenGenericSuccess_ReturnsOkObjectResultWithValue()
    {
        // Arrange
        var controller = new DummyController();

        // Act
        ActionResult actionResult = controller.ToActionResult(Result<string>.Success("ok"));

        // Assert
        OkObjectResult ok = actionResult.Should().BeOfType<OkObjectResult>().Subject;
        ok.Value.Should().Be("ok");
    }

    [Theory]
    [InlineData(ErrorType.Validation, StatusCodes.Status400BadRequest, "Bad Request")]
    [InlineData(ErrorType.Unauthorized, StatusCodes.Status401Unauthorized, "Unauthorized")]
    [InlineData(ErrorType.Forbidden, StatusCodes.Status403Forbidden, "Forbidden")]
    [InlineData(ErrorType.NotFound, StatusCodes.Status404NotFound, "Not Found")]
    [InlineData(ErrorType.Conflict, StatusCodes.Status409Conflict, "Conflict")]
    [InlineData(ErrorType.Failure, StatusCodes.Status500InternalServerError, "Internal Server Error")]
    public void ToActionResult_WhenFailure_ReturnsMappedObjectResult(
        ErrorType errorType,
        int expectedStatus,
        string expectedTitle)
    {
        // Arrange
        var controller = new DummyController();
        var error = new Error("Test.Code", "Test message", errorType);

        // Act
        ActionResult actionResult = controller.ToActionResult(Result.Failure(error));

        // Assert
        ObjectResult objectResult = actionResult.Should().BeOfType<ObjectResult>().Subject;
        objectResult.StatusCode.Should().Be(expectedStatus);
        ProblemDetails details = objectResult.Value.Should().BeOfType<ProblemDetails>().Subject;
        details.Title.Should().Be(expectedTitle);
        details.Detail.Should().Be("Test message");
        details.Extensions["code"].Should().Be("Test.Code");
    }

    [Fact]
    public void ToIResult_WhenSuccess_ReturnsExpectedHttpResults()
    {
        // Act
        IResult nonGeneric = Result.Success().ToIResult();
        IResult generic = Result<int>.Success(42).ToIResult();

        // Assert
        nonGeneric.Should().BeOfType<NoContent>();
        Ok<int> ok = generic.Should().BeOfType<Ok<int>>().Subject;
        ok.Value.Should().Be(42);
    }

    [Theory]
    [InlineData(ErrorType.Validation, StatusCodes.Status400BadRequest)]
    [InlineData(ErrorType.Unauthorized, StatusCodes.Status401Unauthorized)]
    [InlineData(ErrorType.Forbidden, StatusCodes.Status403Forbidden)]
    [InlineData(ErrorType.NotFound, StatusCodes.Status404NotFound)]
    [InlineData(ErrorType.Conflict, StatusCodes.Status409Conflict)]
    [InlineData(ErrorType.Failure, StatusCodes.Status500InternalServerError)]
    public void ToIResult_WhenFailure_ReturnsProblemHttpResultWithExpectedStatus(
        ErrorType errorType,
        int expectedStatus)
    {
        // Arrange
        var error = new Error("Test.Code", "Failure detail", errorType);

        // Act
        IResult result = Result.Failure(error).ToIResult();

        // Assert
        ProblemHttpResult problem = result.Should().BeOfType<ProblemHttpResult>().Subject;
        problem.StatusCode.Should().Be(expectedStatus);
        problem.ProblemDetails.Detail.Should().Be("Failure detail");
        problem.ProblemDetails.Extensions["code"].Should().Be("Test.Code");
    }
}
