using Contracts.Abstractions;
using Device.Domain.Entities;

namespace Device.Domain.Specifications;

public sealed class OfflineControllersSpecification 
    : BaseSpecification<Controller>
{
    public OfflineControllersSpecification(DateTime offlineThreshold)
        : base(data => data.IsOnline && data.LastSeenAt < offlineThreshold)
    {
        
    }
}
