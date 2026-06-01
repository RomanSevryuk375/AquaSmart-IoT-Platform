using AutoMapper;
using Contracts.Results;
using Control.Domain.Interfaces;
using MediatR;

namespace Control.Application.CQRS.Ecosystem.Queries.GetEcosystemById;

public sealed class GetEcosystemByIdHandler(
    IEcosystemRepository ecosystemRepository,
    IMapper mapper) : IRequestHandler<GetEcosystemByIdQuery, Result<EcosystemDto>>
{
    public async Task<Result<EcosystemDto>> Handle(
        GetEcosystemByIdQuery request, 
        CancellationToken cancellationToken)
    {
        var ecosystem = await ecosystemRepository.GetByIdAsync(request.EcosystemId, cancellationToken);
        if (ecosystem is null)
        {
            return Result<EcosystemDto>.Failure(Error.NotFound(
                "Ecosystem.NotFound", $"Ecosystem {request.EcosystemId} not found"));
        }

        return Result<EcosystemDto>.Success(mapper.Map<EcosystemDto>(ecosystem));
    }
}
