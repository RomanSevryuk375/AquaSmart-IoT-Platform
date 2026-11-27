using Mobile.Services.Dto.Control;
using Mobile.Services.Interfaces;

namespace Mobile.Services;

public sealed class VacationModeService : IVacationModeService
{
    private readonly IApiClient _apiClient;
    private readonly object _syncLock = new();
    private readonly List<VacationModeDto> _vacationModes = new();

    public VacationModeService(IApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    public async Task<List<VacationModeDto>> GetVacationModesAsync(Guid ecosystemId, CancellationToken cancellationToken = default)
    {
        try
        {
            var route = $"{MobileApiEndpoints.VacationModes}?ecosystemId={ecosystemId}";
            var response = await _apiClient.GetAsync<List<VacationModeDto>>(route, cancellationToken);
            if (response.IsSuccess && response.Data != null)
            {
                lock (_syncLock)
                {
                    _vacationModes.RemoveAll(x => x.EcosystemId == ecosystemId);
                    _vacationModes.AddRange(response.Data);
                }
                return response.Data;
            }
        }
        catch
        {
        }

        lock (_syncLock)
        {
            return _vacationModes.Where(x => x.EcosystemId == ecosystemId).ToList();
        }
    }

    public async Task<bool> CreateVacationModeAsync(CreateVacationModeRequestDto dto, CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _apiClient.PostAsync(MobileApiEndpoints.VacationModes, dto, cancellationToken);
            if (response.IsSuccess)
            {
                await GetVacationModesAsync(dto.EcosystemId, cancellationToken);
                return true;
            }
        }
        catch
        {
        }

        lock (_syncLock)
        {
            _vacationModes.Add(new VacationModeDto
            {
                Id = Guid.NewGuid(),
                EcosystemId = dto.EcosystemId,
                DateRange = $"{dto.StartDate:dd.MM.yyyy} - {dto.EndDate:dd.MM.yyyy}",
                CalculatedFeed = dto.CalculatedFeed,
                IsActive = dto.IsActive,
                CreatedAt = DateTime.UtcNow
            });
        }
        return true;
    }

    public async Task<bool> ToggleVacationModeAsync(Guid vacationModeId, CancellationToken cancellationToken = default)
    {
        try
        {
            var route = $"{MobileApiEndpoints.VacationModes}/{vacationModeId}/toggle";
            var response = await _apiClient.PutAsync(route, new { }, cancellationToken);
            if (response.IsSuccess)
            {
                return true;
            }
        }
        catch
        {
        }

        lock (_syncLock)
        {
            var vac = _vacationModes.FirstOrDefault(x => x.Id == vacationModeId);
            if (vac != null)
            {
                var idx = _vacationModes.IndexOf(vac);
                _vacationModes[idx] = vac with { IsActive = !vac.IsActive };
                return true;
            }
        }
        return false;
    }

    public async Task<bool> DeleteVacationModeAsync(Guid vacationModeId, CancellationToken cancellationToken = default)
    {
        try
        {
            var route = $"{MobileApiEndpoints.VacationModes}/{vacationModeId}";
            var response = await _apiClient.DeleteAsync(route, cancellationToken);
            if (response.IsSuccess)
            {
                return true;
            }
        }
        catch
        {
        }

        lock (_syncLock)
        {
            _vacationModes.RemoveAll(x => x.Id == vacationModeId);
        }
        return true;
    }

    public void RemoveVacationModesForAquarium(Guid aquariumId)
    {
        lock (_syncLock)
        {
            _vacationModes.RemoveAll(x => x.EcosystemId == aquariumId);
        }
    }
}
