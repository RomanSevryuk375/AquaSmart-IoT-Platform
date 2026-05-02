using Contracts.Abstractions;
using Control.Domain.Entities;
using Control.Domain.SpecificationParams;

namespace Control.Domain.Specifications;

public class EcosystemFilterSpecification : BaseSpecification<EcosystemEntity>
{
    public EcosystemFilterSpecification(EcosystemFilterParams @params) 
        : base(data => 
            (!@params.ControllerId.HasValue || data.ControllerId == @params.ControllerId.Value) &&
            (!@params.Type.HasValue || data.Type == @params.Type.Value) &&
            (!@params.UserId.HasValue || data.UserId == @params.UserId.Value) &&
            (string.IsNullOrWhiteSpace(@params.Name)
                || data.Name.ToLower().Contains(@params.Name.ToLower())))
    {
    }
}
