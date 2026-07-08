using System.Data;
using Contracts.Results;
using Dapper;
using MediatR;
using Notification.Application.Features.Notifications.Queries.Shared;
using Notification.Domain.Interfaces;

namespace Notification.Application.Features.Notifications.Queries.GetAllNotifications;

public sealed class GetAllNotificationsHandler(ISqlConnectionFactory sqlConnectionFactory)
    : IRequestHandler<GetAllNotificationsQuery, Result<IReadOnlyList<NotificationDto>>>
{
    public async Task<Result<IReadOnlyList<NotificationDto>>> Handle(
        GetAllNotificationsQuery request,
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
            WHERE user_id = @UserId
              AND (@EcosystemId IS NULL OR ecosystem_id = @EcosystemId)
              AND (@Level IS NULL OR level = @Level)
              AND (@IsRead IS NULL OR is_read = @IsRead)
              AND (@SearchTerm IS NULL OR @SearchTerm = '' OR message ILIKE '%' || @SearchTerm || '%')
            ORDER BY created_at DESC
            LIMIT @Take OFFSET @Skip
            """;

        IEnumerable<NotificationDto> notifications = await connection.QueryAsync<NotificationDto>(SQL, request);

        return Result<IReadOnlyList<NotificationDto>>.Success(notifications.ToList().AsReadOnly());
    }
}
