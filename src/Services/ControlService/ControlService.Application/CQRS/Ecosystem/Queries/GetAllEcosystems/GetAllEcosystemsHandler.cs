using AutoMapper;
using Control.Domain.Interfaces;
using Control.Domain.SpecificationParams;
using Control.Domain.Specifications;
using MediatR;

namespace Control.Application.CQRS.Ecosystem.Queries.GetAllEcosystems;

public sealed class GetAllEcosystemsHandler(
    IEcosystemRepository ecosystemRepository, 
    IMapper mapper) : IRequestHandler<GetAllEcosystemsQuery, IReadOnlyList<EcosystemDto>>
{
    public async Task<IReadOnlyList<EcosystemDto>> Handle(
        GetAllEcosystemsQuery request, 
        CancellationToken cancellationToken)
    {
        var specification = new EcosystemFilterSpecification(
            new EcosystemFilterParams
            {
                UserId = request.UserId,
                Name = request.Name,
                ControllerId = request.ControllerId,
                Type = request.Type,
            });

        var ecosystems = await ecosystemRepository.GetAllAsync(
            specification,
            request.Skip,
            request.Take,
            cancellationToken);

        return mapper.Map<IReadOnlyList<EcosystemDto>>(ecosystems);
    }
}
