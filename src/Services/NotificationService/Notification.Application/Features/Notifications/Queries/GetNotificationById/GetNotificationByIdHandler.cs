using System.Data;
using Contracts.Results;
using Dapper;
using MediatR;
using Notification.Application.Features.Notifications.Queries.Shared;
using Notification.Domain.Interfaces;

namespace Notification.Application.Features.Notifications.Queries.GetNotificationById;

public sealed class GetNotificationByIdHandler(ISqlConnectionFactory sqlConnectionFactory)
    : IRequestHandler<GetNotificationByIdQuery, Result<NotificationDto>>
{
    public async Task<Result<NotificationDto>> Handle(
        GetNotificationByIdQuery request,
        CancellationToken cancellationToken)
    {
        using IDbConnection connection = sqlConnectionFactory.CreateConnection();

        const string SQL = """
            SELECT 
                id AS Id, 
                user_id AS UserId, 
                ecosystem_id AS EcosystemId, 
                level AS Level, 
                message AS Message, 
                is_read AS IsRead, 
                created_at AS CreatedAt
            FROM notifications
            WHERE id = @NotificationId 
              AND user_id = @UserId
            LIMIT 1
            """;

        NotificationDto? notification = await connection.QuerySingleOrDefaultAsync<NotificationDto>(SQL, request);

        if (notification is null)
        {
            return Result<NotificationDto>.Failure(Error.NotFound<Domain.Entities.Notification>(
                $"Notification {request.NotificationId} not found."));
        }

        return Result<NotificationDto>.Success(notification);
    }
}
