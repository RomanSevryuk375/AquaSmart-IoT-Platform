using Mobile.Services.Dto.Notification;
using Mobile.Services.Interfaces;

namespace Mobile.Services;

public sealed class MaintenanceService : IMaintenanceService
{
    private readonly IApiClient _apiClient;
    private readonly object _syncLock = new();
    private readonly List<MaintenanceLogDto> _maintenanceLogs = new();

    public MaintenanceService(IApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    public async Task<List<MaintenanceLogDto>> GetMaintenanceLogsAsync(Guid ecosystemId, CancellationToken cancellationToken = default)
    {
        try
        {
            var route = $"{MobileApiEndpoints.MaintenanceLogs}?ecosystemId={ecosystemId}";
            var response = await _apiClient.GetAsync<List<MaintenanceLogDto>>(route, cancellationToken);
            if (response.IsSuccess && response.Data != null)
            {
                lock (_syncLock)
                {
                    _maintenanceLogs.RemoveAll(x => x.EcosystemId == ecosystemId);
                    _maintenanceLogs.AddRange(response.Data);
                }
                return response.Data.OrderByDescending(x => x.ActionDate).ToList();
            }
        }
        catch
        {
        }

        lock (_syncLock)
        {
            return _maintenanceLogs.Where(x => x.EcosystemId == ecosystemId).OrderByDescending(x => x.ActionDate).ToList();
        }
    }

    public async Task<bool> AddMaintenanceLogAsync(CreateMaintenanceLogRequestDto dto, CancellationToken cancellationToken = default)
    {
        try
        {
            var payload = dto with { Metrics = dto.Metrics ?? new Dictionary<string, double>() };
            var response = await _apiClient.PostAsync(MobileApiEndpoints.MaintenanceLogs, payload, cancellationToken);
            if (response.IsSuccess)
            {
                await GetMaintenanceLogsAsync(dto.EcosystemId, cancellationToken);
                return true;
            }
        }
        catch
        {
        }

        lock (_syncLock)
        {
            _maintenanceLogs.Add(new MaintenanceLogDto
            {
                Id = Guid.NewGuid(),
                EcosystemId = dto.EcosystemId,
                ActionDate = dto.ActionDate,
                Notes = dto.Notes,
                Metrics = dto.Metrics ?? new(),
                CreatedAt = DateTime.UtcNow
            });
        }
        return true;
    }

    public async Task<bool> DeleteMaintenanceLogAsync(Guid logId, CancellationToken cancellationToken = default)
    {
        try
        {
            var route = $"{MobileApiEndpoints.MaintenanceLogs}/{logId}";
            await _apiClient.DeleteAsync(route, cancellationToken);
        }
        catch
        {
        }

        lock (_syncLock)
        {
            _maintenanceLogs.RemoveAll(x => x.Id == logId);
        }
        return true;
    }

    public void RemoveMaintenanceLogsForAquarium(Guid aquariumId)
    {
        lock (_syncLock)
        {
            _maintenanceLogs.RemoveAll(x => x.EcosystemId == aquariumId);
        }
    }
}
