using Contracts.Abstractions;
using Contracts.Enums;
using Telemetry.Domain.Entities;

namespace Telemetry.Domain.Specifications;

public sealed class SensorIsDelayedSpecification 
    : BaseSpecification<SensorEntity>
{
    public SensorIsDelayedSpecification(
        DateTime offlineThreshold) : 
        base(data => 
            data.UpdatedAt < offlineThreshold && 
            data.State == SensorStateEnum.Active)
    {   
    }
}
