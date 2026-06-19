namespace Device.Domain.Specifications;

public sealed class OfflineControllersSpecification(DateTime offlineThreshold)
        : BaseSpecification<Controller>(data => data.IsOnline && data.LastSeenAt < offlineThreshold)
{
}
