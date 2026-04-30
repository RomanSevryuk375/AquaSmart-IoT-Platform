using Contracts.Abstractions;
using Control.Domain.Entities;
using Control.Domain.SpecificationParams;

namespace Control.Domain.Specifications;

public class RelayFilterSpecification : BaseSpecification<RelayEntity>
{
    public RelayFilterSpecification(RelayFilterParams @params) 
        : base(data => 
            (!@params.EcosystemId.HasValue || data.EcosystemId == @params.EcosystemId.Value)
            && (!@params.ControllerId.HasValue || data.ControllerId == @params.ControllerId.Value)
            && (!@params.PowerSensorId.HasValue || data.PowerSensorId == @params.PowerSensorId.Value)
            && (!@params.Purpose.HasValue || data.Purpose == @params.Purpose.Value)
            && (!@params.IsManual.HasValue || data.IsManual == @params.IsManual.Value)
            && (!@params.IsActive.HasValue || data.IsActive == @params.IsActive.Value))
    {
        
    }
}
