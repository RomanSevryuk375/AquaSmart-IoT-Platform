using AutoMapper;
using Contracts.Results;
using Control.Application.Interfaces;
using MediatR;

namespace Control.Application.CQRS.Ecosystem.Queries.GetEcosystemById;

public sealed class GetEcosystemByIdHandler(
    ISecureService secureService,
    IMapper mapper) : IRequestHandler<GetEcosystemByIdQuery, Result<EcosystemDto>>
{
    public async Task<Result<EcosystemDto>> Handle(
        GetEcosystemByIdQuery request, 
        CancellationToken cancellationToken)
    {
        var ownership = await secureService.EnsureUserOwnsEcosystemAsync(
            request.EcosystemId, cancellationToken);
        if (ownership.IsFailure)
        {
            return Result<EcosystemDto>.Failure(ownership.Error);
        }

        return Result<EcosystemDto>.Success(mapper.Map<EcosystemDto>(ownership.Value));
    }
}
