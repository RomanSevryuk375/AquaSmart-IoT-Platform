using BuildingBlocks.Domain.Results;
using BuildingBlocks.Presentation.Constants;
using BuildingBlocks.Presentation.Endpoints;
using BuildingBlocks.Presentation.ResultExtensions;
using IdentityService.Application.DTOs;
using IdentityService.Application.Features.Auth.Commands.Login;
using IdentityService.Application.Interfaces;
using MediatR;

namespace IdentityService.API.Endpoints.Auth;

public sealed class LoginEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost($"{ApiConstants.Routes.Auth}/login", async (
            LoginUserRequestDto request,
            ISender sender,
            HttpContext context,
            ICookieHelpers cookieHelpers,
            CancellationToken cancellationToken) =>
        {
            var command = new LoginCommand
            {
                Email = request.Email,
                Password = request.Password
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
        .AllowAnonymous();
    }
}
