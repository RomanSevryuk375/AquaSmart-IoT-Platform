using Mobile.Services.Dto.Control;
using Mobile.Services.Interfaces;

namespace Mobile.Services;

public sealed class ScheduleService : IScheduleService
{
    private readonly IApiClient _apiClient;
    private readonly object _syncLock = new();
    private readonly List<ScheduleDto> _schedules = new();

    public ScheduleService(IApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    public async Task<List<ScheduleDto>> GetSchedulesAsync(Guid ecosystemId, CancellationToken cancellationToken = default)
    {
        try
        {
            var route = $"{MobileApiEndpoints.Schedules}?ecosystemId={ecosystemId}";
            var response = await _apiClient.GetAsync<List<ScheduleDto>>(route, cancellationToken);
            if (response.IsSuccess && response.Data != null)
            {
                lock (_syncLock)
                {
                    _schedules.RemoveAll(x => x.EcosystemId == ecosystemId);
                    _schedules.AddRange(response.Data);
                }
                return response.Data;
            }
        }
        catch
        {
        }

        lock (_syncLock)
        {
            return _schedules.Where(x => x.EcosystemId == ecosystemId).ToList();
        }
    }

    public async Task<bool> CreateScheduleAsync(CreateScheduleRequestDto dto, CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _apiClient.PostAsync(MobileApiEndpoints.Schedules, dto, cancellationToken);
            if (response.IsSuccess)
            {
                await GetSchedulesAsync(dto.EcosystemId, cancellationToken);
                return true;
            }
        }
        catch
        {
        }

        lock (_syncLock)
        {
            _schedules.Add(new ScheduleDto
            {
                Id = Guid.NewGuid(),
                EcosystemId = dto.EcosystemId,
                RelayId = dto.RelayId,
                CronExpression = dto.CronExpression,
                DurationMin = dto.DurationMin,
                IsEnabled = dto.IsEnabled,
                IsFadeMode = dto.IsFadeMode,
                CreatedAt = DateTime.UtcNow
            });
        }
        return true;
    }

    public async Task<bool> ToggleScheduleAsync(Guid scheduleId, bool isActive, CancellationToken cancellationToken = default)
    {
        try
        {
            var route = $"{MobileApiEndpoints.Schedules}/{scheduleId}/active";
            var response = await _apiClient.PutAsync(route, new { isActive }, cancellationToken);
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
            var s = _schedules.FirstOrDefault(x => x.Id == scheduleId);
            if (s != null)
            {
                var idx = _schedules.IndexOf(s);
                _schedules[idx] = s with { IsEnabled = isActive };
                return true;
            }
        }
        return false;
    }

    public async Task<bool> DeleteScheduleAsync(Guid scheduleId, CancellationToken cancellationToken = default)
    {
        try
        {
            var route = $"{MobileApiEndpoints.Schedules}/{scheduleId}";
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
            _schedules.RemoveAll(x => x.Id == scheduleId);
        }
        return true;
    }

    public void RemoveSchedulesForAquarium(Guid aquariumId)
    {
        lock (_syncLock)
        {
            _schedules.RemoveAll(x => x.EcosystemId == aquariumId);
        }
    }
}
