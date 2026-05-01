using Contracts.Abstractions;
using Notification.Domain.Entities;
using Notification.Domain.SpecificationParams;

namespace Notification.Domain.Specifications;

public class NotificationFilterSpecification 
    : BaseSpecification<NotificationEntity>
{
    public NotificationFilterSpecification(
        NotificationSpecificationParams @params)
        : base(data =>
            (!@params.UserId.HasValue || data.UserId == @params.UserId.Value) &&
            (!@params.EcosystemId.HasValue || data.EcosystemId == @params.EcosystemId.Value) &&
            (!@params.Level.HasValue || data.Level == @params.Level.Value) &&
            (!@params.IsRead.HasValue || data.IsRead == @params.IsRead.Value) &&
            (string.IsNullOrWhiteSpace(@params.SearchTerm) ||
             data.Message.ToLower().Contains(@params.SearchTerm.ToLower())))
    {
    }
}
