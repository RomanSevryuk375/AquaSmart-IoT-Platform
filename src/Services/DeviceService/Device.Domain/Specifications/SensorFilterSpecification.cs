using Contracts.Abstractions;
using Device.Domain.Entities.Sensors;
using Device.Domain.SpecificationParams;

namespace Device.Domain.Specifications;

public sealed class SensorFilterSpecification 
    : BaseSpecification<Sensor>
{
    public SensorFilterSpecification(SensorFilterParams @params) 
        : base(data => 
            (!@params.ControllerId.HasValue || data.ControllerId == @params.ControllerId.Value) &&
            (!@params.UserId.HasValue || data.UserId == @params.UserId.Value) &&
            (!@params.Type.HasValue || data.Type == @params.Type.Value) &&
            (!@params.State.HasValue || data.State == @params.State.Value))
    {
    }
}
