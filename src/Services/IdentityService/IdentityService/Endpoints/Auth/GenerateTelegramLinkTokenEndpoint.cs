using BuildingBlocks.Domain.Abstractions;
using BuildingBlocks.Domain.Results;
using BuildingBlocks.Presentation.Authorization;
using BuildingBlocks.Presentation.Constants;
using BuildingBlocks.Presentation.Endpoints;
using BuildingBlocks.Presentation.ResultExtensions;
using IdentityService.Application.DTOs;
using IdentityService.Application.Features.Auth.Commands.GenerateTelegramLink;
using MediatR;

namespace IdentityService.API.Endpoints.Auth;

public sealed class GenerateTelegramLinkTokenEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost($"{ApiConstants.Routes.Profiles}/telegram-link-token", async (
            ISender sender,
            IUserContext userContext,
            CancellationToken cancellationToken) =>
        {
            var command = new GenerateTelegramLinkCommand
            {
                UserId = userContext.UserId
            };

            Result<TelegramLinkTokenResponseDto> result = await sender.Send(command, cancellationToken);

            return result.ToIResult();
        })
        .WithTags("Profile")
        .Produces<TelegramLinkTokenResponseDto>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status401Unauthorized)
        .RequireAuthorization(SubPermissions.AccountUpdate);
    }
}
