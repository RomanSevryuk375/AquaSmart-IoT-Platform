using BuildingBlocks.Domain.Results;
using BuildingBlocks.Presentation.Constants;
using BuildingBlocks.Presentation.Endpoints;
using BuildingBlocks.Presentation.ResultExtensions;
using IdentityService.Application.DTOs;
using IdentityService.Application.Features.Auth.Commands.Refresh;
using IdentityService.Application.Interfaces;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace IdentityService.API.Endpoints.Auth;

public sealed class RefreshEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost($"{ApiConstants.Routes.Auth}/refresh", async (
            [FromBody] RefreshTokenRequestDto? request,
            ISender sender,
            HttpContext context,
            ICookieHelpers cookieHelpers,
            CancellationToken cancellationToken) =>
        {
            string? refreshToken = request?.RefreshToken;

            if (string.IsNullOrWhiteSpace(refreshToken))
            {
                context.Request.Cookies.TryGetValue(AuthConstants.RefreshTokenCookieName, out refreshToken);
            }

            var command = new RefreshCommand
            {
                RefreshToken = refreshToken ?? string.Empty
            };

            Result<LoginResponseDto> result = await sender.Send(command, cancellationToken);

            if (result.IsSuccess)
            {
                cookieHelpers.AppendAuthCookies(context, result.Value);
            }

            return result.ToIResult();
        })
        .WithTags("Auth")
        .Produces<LoginResponseDto>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status401Unauthorized)
        .Produces(StatusCodes.Status404NotFound)
        .AllowAnonymous();
    }
}
