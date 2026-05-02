namespace IdentityService.Application.Interfaces;

public interface ISubscriptionExpiredChecker
{
    Task CheckAsync(CancellationToken cancellationToken);
}