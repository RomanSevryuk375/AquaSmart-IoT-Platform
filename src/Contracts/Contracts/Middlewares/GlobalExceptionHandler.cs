using System.ComponentModel.DataAnnotations;
using Contracts.Exceptions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace Contracts.Middlewares;

public class GlobalExceptionHandler(RequestDelegate next, ILogger<GlobalExceptionHandler> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";

        int statusCode = exception switch
        {
            NotFoundException => StatusCodes.Status404NotFound,
            ValidationException => StatusCodes.Status409Conflict,
            DomainValidationException => StatusCodes.Status409Conflict,
            KeyNotFoundException => StatusCodes.Status404NotFound,
            ArgumentException => StatusCodes.Status400BadRequest,
            EmailIsBusyException => StatusCodes.Status400BadRequest,
            RegisterException => StatusCodes.Status409Conflict,
            InvalidCredentialsException => StatusCodes.Status400BadRequest,
            UnauthorizedAccessException => StatusCodes.Status401Unauthorized,

            _ => StatusCodes.Status500InternalServerError
        };

        if (statusCode == StatusCodes.Status500InternalServerError)
        {
            logger.LogError(exception, "Unhandled exception occurred while processing request {Path}", context.Request.Path);
        }
        else
        {
            logger.LogWarning("Expected business/domain exception occurred while processing request {Path}: {Message}", context.Request.Path, exception.Message);
        }

        context.Response.StatusCode = statusCode;

        var response = new
        {
            StatusCode = statusCode,
            Message = exception.Message,
        };

        return context.Response.WriteAsJsonAsync(response);
    }
}

public static class UseMiddleware
{
    public static IApplicationBuilder UseGlobalExceptionHandler(this IApplicationBuilder builder) =>
        builder.UseMiddleware<GlobalExceptionHandler>();
}
