using BuildingBlocks.Domain.Abstractions;
using BuildingBlocks.Domain.Results;
using BuildingBlocks.Presentation.Authorization;
using BuildingBlocks.Presentation.Constants;
using BuildingBlocks.Presentation.Endpoints;
using BuildingBlocks.Presentation.ResultExtensions;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Telemetry.Application.DTOs;
using Telemetry.Application.Features.Telemetry.Queries.GetAggregatedTelemetryChart;

namespace Telemetry.API.Endpoints;

public sealed class GetAggregatedDataEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet($"{ApiConstants.Routes.Data}/aggregate", async (
            [AsParameters] TelemetryAggregateFilterDto filter,
            ISender sender,
            IUserContext userContext,
            [FromQuery] int skip = 0,
            [FromQuery] int take = 10,
            CancellationToken cancellationToken = default) =>
        {
            var query = new GetAggregatedTelemetryChartQuery
            {
                SensorId = filter.SensorId,
                UserId = userContext.UserId,
                Period = filter.Period,
                From = filter.From,
                To = filter.To,
                Skip = skip,
                Take = take
            };

            Result<TelemetryChartResponseDto> result = await sender.Send(query, cancellationToken);

            return result.ToIResult();
        })
        .WithTags("Telemetry Data")
        .Produces<TelemetryChartResponseDto>()
        .Produces(StatusCodes.Status404NotFound)
        .RequireAuthorization(SubPermissions.AnalyticsHistory);
    }
}
