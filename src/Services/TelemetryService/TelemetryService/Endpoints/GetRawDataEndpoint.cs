using BuildingBlocks.Domain.Abstractions;
using BuildingBlocks.Domain.Results;
using BuildingBlocks.Presentation.Authorization;
using BuildingBlocks.Presentation.Constants;
using BuildingBlocks.Presentation.Endpoints;
using BuildingBlocks.Presentation.ResultExtensions;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Telemetry.Application.DTOs;
using Telemetry.Application.Features.Telemetry.Queries.GetRawTelemetryChart;

namespace Telemetry.API.Endpoints;

public sealed class GetRawDataEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet($"{ApiConstants.Routes.Data}/raw", async (
            [AsParameters] TelemetryDataFilterDto filter,
            ISender sender,
            IUserContext userContext,
            [FromQuery] int skip = 0,
            [FromQuery] int take = 10,
            CancellationToken cancellationToken = default) =>
        {
            var query = new GetRawTelemetryChartQuery
            {
                SensorId = filter.SensorId,
                UserId = userContext.UserId,
                From = filter.From,
                To = filter.To,
                Skip = skip,
                Take = take
            };

            Result<TelemetryRawChartResponseDto> result = await sender.Send(query, cancellationToken);

            return result.ToIResult();
        })
        .WithTags("Telemetry Data")
        .Produces<TelemetryRawChartResponseDto>()
        .Produces(StatusCodes.Status404NotFound)
        .RequireAuthorization(SubPermissions.DataRealtime);
    }
}
