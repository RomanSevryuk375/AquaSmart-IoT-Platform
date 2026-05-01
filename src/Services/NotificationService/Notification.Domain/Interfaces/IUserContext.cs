namespace Notification.Domain.Interfaces;

public interface IUserContext
{
    bool IsAuthenticated { get; }
    Guid UserId { get; }
}