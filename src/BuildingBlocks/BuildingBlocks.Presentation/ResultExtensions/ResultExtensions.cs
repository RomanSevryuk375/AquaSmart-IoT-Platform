using BuildingBlocks.Domain.Results;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace BuildingBlocks.Presentation.ResultExtensions;

public static class ResultExtensions
{
    public static ActionResult ToActionResult<T>(
        this ControllerBase controller, Result<T> result)
    {
        if (result.IsSuccess)
        {
            return controller.Ok(result.Value);
        }

        return MapToObjectResult(result.Error);
    }

    public static ActionResult ToActionResult(
        this ControllerBase controller, Result result)
    {
        if (result.IsSuccess)
        {
            return controller.NoContent();
        }

        return MapToObjectResult(result.Error);
    }

    public static IResult ToIResult(this Result result)
    {
        if (result.IsSuccess)
        {
            return Results.NoContent();
        }

        return MapToIResult(result.Error);
    }

    public static IResult ToIResult<T>(this Result<T> result)
    {
        if (result.IsSuccess)
        {
            return Results.Ok(result.Value);
        }

        return MapToIResult(result.Error);
    }

    private static ObjectResult MapToObjectResult(Error error)
    {
        int statusCode = GetStatusCode(error.Type);
        var problemDetails = new ProblemDetails
        {
            Status = statusCode,
            Title = GetTitle(error.Type),
            Detail = error.Message,
            Extensions =
            {
                ["code"] = error.Code,
                ["error"] = error.Code,
                ["message"] = error.Message
            }
        };

        return new ObjectResult(problemDetails) { StatusCode = statusCode };
    }

    private static IResult MapToIResult(Error error)
    {
        int statusCode = GetStatusCode(error.Type);
        return Results.Problem(
            detail: error.Message,
            statusCode: statusCode,
            title: GetTitle(error.Type),
            extensions: new Dictionary<string, object?>
            {
                ["code"] = error.Code,
                ["error"] = error.Code,
                ["message"] = error.Message
            });
    }

    private static int GetStatusCode(ErrorType errorType) => errorType switch
    {
        ErrorType.Validation => StatusCodes.Status400BadRequest,
        ErrorType.Unauthorized => StatusCodes.Status401Unauthorized,
        ErrorType.Forbidden => StatusCodes.Status403Forbidden,
        ErrorType.NotFound => StatusCodes.Status404NotFound,
        ErrorType.Conflict => StatusCodes.Status409Conflict,
        _ => StatusCodes.Status500InternalServerError
    };

    private static string GetTitle(ErrorType errorType) => errorType switch
    {
        ErrorType.Validation => "Bad Request",
        ErrorType.Unauthorized => "Unauthorized",
        ErrorType.Forbidden => "Forbidden",
        ErrorType.NotFound => "Not Found",
        ErrorType.Conflict => "Conflict",
        _ => "Internal Server Error"
    };
}
