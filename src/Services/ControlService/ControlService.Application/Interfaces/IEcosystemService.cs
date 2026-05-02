using Contracts.Results;
using Control.Application.DTOs.Ecosystem;

namespace Control.Application.Interfaces;

public interface IEcosystemService
{
    Task<Result<Guid>> CreateEcosystemAsync(
        EcosystemRequestDto request, 
        CancellationToken cancellationToken);

    Task<Result> DeleteEcosystemAsync(
        Guid ecosystemId, 
        CancellationToken cancellationToken);

    Task<IReadOnlyList<EcosystemResponseDto>> GetAllEcosystemsAsync(
        EcosystemFilterDto filter,
        int? skip, 
        int? take, 
        CancellationToken cancellationToken);

    Task<Result<EcosystemResponseDto>> GetEcosystemByIdAsync(
        Guid ecosystemId,
        CancellationToken cancellationToken);

    Task<Result> UpdateEcosystemAsync(
        Guid ecosystemId, 
        EcosystemUpdateRequestDto request, 
        CancellationToken cancellationToken);
}