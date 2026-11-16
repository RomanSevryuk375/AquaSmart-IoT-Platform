using Mobile.Services.Dto.Control;

namespace Mobile.Services.Interfaces;

public interface IVacationModeService
{
    Task<List<VacationModeDto>> GetVacationModesAsync(Guid ecosystemId, CancellationToken cancellationToken = default);
    Task<bool> CreateVacationModeAsync(CreateVacationModeRequestDto dto, CancellationToken cancellationToken = default);
    Task<bool> ToggleVacationModeAsync(Guid vacationModeId, CancellationToken cancellationToken = default);
    Task<bool> DeleteVacationModeAsync(Guid vacationModeId, CancellationToken cancellationToken = default);
    void RemoveVacationModesForAquarium(Guid aquariumId);
}
