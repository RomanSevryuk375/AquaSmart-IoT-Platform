using Contracts.Abstractions;
using Control.Domain.Entities;
using Control.Domain.SpecificationParams;

namespace Control.Domain.Specifications;

public sealed class SensorFilterSpecification 
    : BaseSpecification<SensorEntity>
{
    public SensorFilterSpecification(SensorFilterParams @params) 
        : base(data => 
            (!@params.EcosystemId.HasValue || data.EcosystemId == @params.EcosystemId.Value)
            && (!@params.ControllerId.HasValue || data.ControllerId == @params.ControllerId.Value)
            && (!@params.State.HasValue || data.State == @params.State.Value)
            && (!@params.Type.HasValue || data.Type == @params.Type.Value))
    {
        
    }
}
