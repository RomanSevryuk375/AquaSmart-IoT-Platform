using BuildingBlocks.Domain.Abstractions;
using BuildingBlocks.Domain.Results;
using BuildingBlocks.Presentation.Authorization;
using BuildingBlocks.Presentation.Constants;
using BuildingBlocks.Presentation.Endpoints;
using BuildingBlocks.Presentation.ResultExtensions;
using MediatR;
using Notification.Application.Features.MaintenanceLogs.Queries.GetAllMaintenanceLogs;
using Notification.Application.Features.MaintenanceLogs.Queries.Shared;

namespace Notification.API.Endpoints.MaintenanceLogs;

public sealed class GetAllLogsEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet(ApiConstants.Routes.MaintenanceLogs, async (
            Guid? ecosystemId,
            DateTime? actionDateFrom,
            DateTime? actionDateTo,
            ISender sender,
            IUserContext userContext,
            int skip = 0,
            int take = 10,
            CancellationToken cancellationToken = default) =>
        {
            var query = new GetAllMaintenanceLogsQuery
            {
                UserId = userContext.UserId,
                EcosystemId = ecosystemId,
                ActionDateFrom = actionDateFrom,
                ActionDateTo = actionDateTo,
                Skip = skip,
                Take = take
            };

            Result<IReadOnlyList<MaintenanceLogDto>> result = await sender.Send(query, cancellationToken);

            return result.ToIResult();
        })
        .WithTags("Maintenance Logs")
        .Produces<IReadOnlyList<MaintenanceLogDto>>()
        .RequireAuthorization(SubPermissions.MaintenanceLogRead);
    }
}
