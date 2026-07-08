namespace Notification.Domain.Interfaces;

public interface IUserContext
{
    public bool IsAuthenticated { get; }
    public Guid UserId { get; }
}
