using BuildingBlocks.Domain.Results;
using BuildingBlocks.Presentation.Constants;
using BuildingBlocks.Presentation.Endpoints;
using BuildingBlocks.Presentation.ResultExtensions;
using IdentityService.API.Filters;
using IdentityService.Application.DTOs;
using IdentityService.Application.Features.Auth.Commands.VerifyTelegram;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace IdentityService.API.Endpoints.Auth;

public sealed class VerifyTelegramEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost($"{ApiConstants.Routes.Auth}/telegram-verify", async (
            [FromBody] VerifyTelegramRequestDto request,
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            var command = new VerifyTelegramCommand
            {
                Token = request.Token,
                ChatId = request.ChatId,
            };

            Result result = await sender.Send(command, cancellationToken);

            return result.ToIResult();
        })
        .WithTags("Auth")
        .Produces(StatusCodes.Status204NoContent)
        .Produces(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status401Unauthorized)
        .Produces(StatusCodes.Status404NotFound)
        .Produces(StatusCodes.Status409Conflict)
        .AddEndpointFilter<HmacValidationFilter>();
    }
}
