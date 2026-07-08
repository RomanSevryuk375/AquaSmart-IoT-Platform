using Contracts.Enums;
using Contracts.Results;
using MediatR;
using Notification.Domain.Entities;
using Notification.Domain.Interfaces;

namespace Notification.Application.Features.Alerts.Commands.SendSubscriptionAlert;

public sealed class SendSubscriptionAlertHandler(
    IUserRepository userRepository,
    INotificationRepository notificationRepository) : IRequestHandler<SendSubscriptionAlertCommand, Result>
{
    public async Task<Result> Handle(SendSubscriptionAlertCommand request, CancellationToken cancellationToken)
    {
        User? user = await userRepository.GetByIdAsync(request.UserId, cancellationToken);
        if (user is null)
        {
            return Result.Failure(Error.NotFound("User.NotFound", $"User {request.UserId} not found."));
        }

        string messageText =
            "Your subscription has expired and was downgraded to Free. " +
            "Premium features and telegram alerts are now disabled.";

        Result<Domain.Entities.Notification> notificationResult = Domain.Entities.Notification.Create(
            notificationId: Guid.NewGuid(), request.UserId, ecosystemId: null,
            level: NotificationLevel.Warning, messageText);

        if (notificationResult.IsFailure)
        {
            return Result.Failure(notificationResult.Error);
        }

        await notificationRepository.AddAsync(notificationResult.Value, cancellationToken);

        return Result.Success();
    }
}
