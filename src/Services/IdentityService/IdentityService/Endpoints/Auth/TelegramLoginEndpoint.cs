using BuildingBlocks.Domain.Results;
using BuildingBlocks.Presentation.Constants;
using BuildingBlocks.Presentation.Endpoints;
using BuildingBlocks.Presentation.ResultExtensions;
using IdentityService.API.Filters;
using IdentityService.Application.DTOs;
using IdentityService.Application.Features.Auth.Commands.TelegramLogin;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace IdentityService.API.Endpoints.Auth;

public sealed class TelegramLoginEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost($"{ApiConstants.Routes.Auth}/telegram-login", async (
            [FromBody] TelegramLoginRequestDto request,
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            var command = new TelegramLoginCommand
            {
                ChatId = request.ChatId,
            };

            Result<TelegramLoginResponseDto> result = await sender.Send(command, cancellationToken);

            return result.ToIResult();
        })
        .WithTags("Auth")
        .Produces<TelegramLoginResponseDto>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status401Unauthorized)
        .Produces(StatusCodes.Status404NotFound)
        .AddEndpointFilter<HmacValidationFilter>();
    }
}
