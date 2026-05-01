using Contracts.Exceptions;
using Notification.Application.DTOs.Notification;
using Notification.Application.Interfaces;
using Notification.Domain.Interfaces;
using Notification.Domain.SpecificationParams;
using Notification.Domain.Specifications;

namespace Notification.Application.Services;

public class NotificationService(
    INotificationRepository notificationRepository,
    IUnitOfWork unitOfWork) : INotificationService
{
    public async Task<IReadOnlyList<NotificationResponseDto>> GetAllNotificationsAsync(
        NotificationFilterDto filter,
        int? skip,
        int? take,
        CancellationToken cancellationToken)
    {
        var specification = new NotificationFilterSpecification(
            new NotificationSpecificationParams
            {
                UserId = filter.UserId,
                EcosystemId = filter.AquariumId,
                Level = filter.Level,
                IsRead = filter.IsRead,
                SearchTerm = filter.SearchTerm,
            });

        var notifications = await notificationRepository.GetAllAsync(
            specification,
            skip,
            take,
            cancellationToken);

        return notifications.Select(n => new NotificationResponseDto
        {
            Id = n.Id,
            UserId = n.UserId,
            AquariumId = n.EcosystemId,
            Level = n.Level,
            Message = n.Message,
            IsRead = n.IsRead,
            CreatedAt = n.CreatedAt,
        }).ToList();
    }

    public async Task<NotificationResponseDto> GetNotificationByIdAsync(
        Guid id,
        CancellationToken cancellationToken)
    {
        var notification = await notificationRepository
            .GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException($"Notification {id} not found");

        return new NotificationResponseDto
        {
            Id = notification.Id,
            UserId = notification.UserId,
            AquariumId = notification.EcosystemId,
            Level = notification.Level,
            Message = notification.Message,
            IsRead = notification.IsRead,
            CreatedAt = notification.CreatedAt,
        };
    }

    public async Task MarkNotificationAsReadAsync(
        Guid id,
        CancellationToken cancellationToken)
    {
        var notification = await notificationRepository
            .GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException($"Notification {id} not found");

        notification.MarkAsRead();

        await notificationRepository.UpdateAsync(notification, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
