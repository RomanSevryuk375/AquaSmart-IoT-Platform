using Contracts.Abstractions;
using Device.Domain.Entities;
using Device.Domain.SpecificationParams;

namespace Device.Domain.Specifications;

public class SensorFilterSpecification : BaseSpecification<SensorEntity>
{
    public SensorFilterSpecification(SensorFilterParams @params) 
        : base(data => 
            (!@params.ControllerId.HasValue || data.ControllerId == @params.ControllerId.Value) &&
            (!@params.Type.HasValue || data.Type == @params.Type.Value) &&
            (!@params.State.HasValue || data.State == @params.State.Value))
    {
    }
}
