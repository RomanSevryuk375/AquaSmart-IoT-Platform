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

        return MapError(controller, result.Error);
    }

    public static ActionResult ToActionResult(
        this ControllerBase controller, Result result)
    {
        if (result.IsSuccess)
        {
            return controller.NoContent();
        }

        return MapError(controller, result.Error);
    }

    public static IResult ToIResult(this Result result)
    {
        if (result.IsSuccess)
        {
            return Results.NoContent();
        }

        return MapError(result.Error);
    }

    public static IResult ToIResult<T>(this Result<T> result)
    {
        if (result.IsSuccess)
        {
            return Results.Ok(result.Value);
        }

        return MapError(result.Error);
    }

    private static ObjectResult MapError(
        ControllerBase controller, Error error)
    {
        var response = new
        {
            error = error.Code,
            message = error.Message
        };

        return error.Type switch
        {
            ErrorType.NotFound => controller.NotFound(response),
            ErrorType.Validation => controller.BadRequest(response),
            ErrorType.Conflict => controller.Conflict(response),
            ErrorType.Forbidden => controller.StatusCode(StatusCodes.Status403Forbidden, response),
            ErrorType.Unauthorized => controller.StatusCode(StatusCodes.Status401Unauthorized, response),

            _ => controller.StatusCode(500, new { error = "InternalError", message = error.Message })
        };
    }

    private static IResult MapError(Error error)
    {
        var response = new { error = error.Code, message = error.Message };

        return error.Type switch
        {
            ErrorType.NotFound => Results.NotFound(response),
            ErrorType.Validation => Results.BadRequest(response),
            ErrorType.Conflict => Results.Conflict(response),
            ErrorType.Forbidden => Results.Json(response, statusCode: StatusCodes.Status403Forbidden),
            ErrorType.Unauthorized => Results.Json(response, statusCode: StatusCodes.Status401Unauthorized),

            _ => Results.Json(response, statusCode: StatusCodes.Status500InternalServerError)
        };
    }
}
