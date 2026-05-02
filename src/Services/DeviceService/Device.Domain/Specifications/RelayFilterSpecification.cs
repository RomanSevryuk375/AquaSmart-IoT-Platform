using Contracts.Abstractions;
using Device.Domain.Entities;
using Device.Domain.SpecificationParams;

namespace Device.Domain.Specifications;

public class RelayFilterSpecification : BaseSpecification<RelayEntity>
{
    public RelayFilterSpecification(RelayFilterParams @params) : 
            base(data => 
                (!@params.ControllerId.HasValue || data.ControllerId == @params.ControllerId.Value) &&
                (!@params.Purpose.HasValue || data.Purpose == @params.Purpose.Value) &&
                (!@params.IsActive.HasValue || data.IsActive == @params.IsActive.Value) &&
                (!@params.IsManual.HasValue || data.IsManual == @params.IsManual.Value))
    {
    }
}

