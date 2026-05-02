using Contracts.Abstractions;
using Device.Domain.Entities;

namespace Device.Domain.Specifications;

public class OfflineControllersSpecification : BaseSpecification<ControllerEntity>
{
    public OfflineControllersSpecification(DateTime offlineThreshold)
        : base(data => data.IsOnline && data.LastSeenAt < offlineThreshold)
    {
        
    }
}
