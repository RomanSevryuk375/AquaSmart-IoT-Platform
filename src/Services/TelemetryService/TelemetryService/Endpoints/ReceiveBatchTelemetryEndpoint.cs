using BuildingBlocks.Domain.Results;
using BuildingBlocks.Presentation.Constants;
using BuildingBlocks.Presentation.Endpoints;
using BuildingBlocks.Presentation.ResultExtensions;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Telemetry.Application.DTOs;
using Telemetry.Application.Features.Telemetry.Commands.AddTelemetryBatch;

namespace Telemetry.API.Endpoints;

public sealed class ReceiveBatchTelemetryEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost($"{ApiConstants.Routes.Data}", async (
            [FromBody] AddTelemetryBatchRequestDto request,
            [FromHeader(Name = ApiConstants.Headers.DeviceToken)] string deviceToken,
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            var command = new AddTelemetryBatchCommand
            {
                MacAddress = request.MacAddress,
                DeviceToken = deviceToken,
                Items = request.Items
            };

            Result result = await sender.Send(command, cancellationToken);

            return result.IsSuccess
                ? Results.Accepted()
                : result.ToIResult();
        })
        .WithTags("Telemetry Data")
        .Produces(StatusCodes.Status202Accepted)
        .Produces(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status401Unauthorized)
        .Produces(StatusCodes.Status403Forbidden)
        .Produces(StatusCodes.Status404NotFound)
        .AllowAnonymous();
    }
}
