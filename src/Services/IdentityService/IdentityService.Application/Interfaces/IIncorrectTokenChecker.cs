namespace IdentityService.Application.Interfaces;

public interface IIncorrectTokenChecker
{
    Task CheckAndDeleteAsync(CancellationToken cancellationToken);
}