using BuildingBlocks.Domain.Results;
using BuildingBlocks.Presentation.Constants;
using BuildingBlocks.Presentation.Endpoints;
using BuildingBlocks.Presentation.ResultExtensions;
using IdentityService.Application.DTOs;
using IdentityService.Application.Features.Auth.Commands.Register;
using IdentityService.Application.Interfaces;
using MediatR;

namespace IdentityService.API.Endpoints.Auth;

public sealed class RegisterEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost($"{ApiConstants.Routes.Auth}/register", async (
            RegisterUserRequestDto request,
            ISender sender,
            HttpContext context,
            ICookieHelpers cookieHelpers,
            CancellationToken cancellationToken) =>
        {
            var command = new RegisterCommand
            {
                Email = request.Email,
                PhoneNumber = request.PhoneNumber,
                Name = request.Name,
                Password = request.Password,
                TimeZone = request.TimeZone
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
        .Produces(StatusCodes.Status409Conflict)
        .AllowAnonymous();
    }
}
