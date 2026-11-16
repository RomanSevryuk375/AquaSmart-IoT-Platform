using Mobile.Services.Dto.Notification;
using Mobile.Services.Interfaces;

namespace Mobile.Services;

public sealed class ReminderService : IReminderService
{
    private readonly IApiClient _apiClient;
    private readonly object _syncLock = new();
    private readonly List<ReminderDto> _reminders = new();

    public ReminderService(IApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    public async Task<List<ReminderDto>> GetRemindersAsync(Guid? ecosystemId = null, CancellationToken cancellationToken = default)
    {
        try
        {
            var route = ecosystemId.HasValue
                ? $"{MobileApiEndpoints.Reminders}?ecosystemId={ecosystemId.Value}"
                : MobileApiEndpoints.Reminders;

            var response = await _apiClient.GetAsync<List<ReminderDto>>(route, cancellationToken);
            if (response.IsSuccess && response.Data != null)
            {
                lock (_syncLock)
                {
                    if (ecosystemId.HasValue)
                    {
                        _reminders.RemoveAll(x => x.EcosystemId == ecosystemId.Value);
                        _reminders.AddRange(response.Data);
                    }
                    else
                    {
                        _reminders.Clear();
                        _reminders.AddRange(response.Data);
                    }
                }
                return response.Data.OrderBy(x => x.NextDueAt).ToList();
            }
        }
        catch
        {
        }

        lock (_syncLock)
        {
            if (ecosystemId.HasValue)
                return _reminders.Where(x => x.EcosystemId == ecosystemId.Value).OrderBy(x => x.NextDueAt).ToList();
            return _reminders.OrderBy(x => x.NextDueAt).ToList();
        }
    }

    public async Task<bool> CreateReminderAsync(CreateReminderRequestDto dto, CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _apiClient.PostAsync(MobileApiEndpoints.Reminders, dto, cancellationToken);
            if (response.IsSuccess)
            {
                await GetRemindersAsync(dto.EcosystemId, cancellationToken);
                return true;
            }
        }
        catch
        {
        }

        lock (_syncLock)
        {
            _reminders.Add(new ReminderDto
            {
                Id = Guid.NewGuid(),
                EcosystemId = dto.EcosystemId,
                TaskName = dto.TaskName,
                IntervalDays = dto.IntervalDays,
                NextDueAt = DateTime.UtcNow.AddDays(dto.IntervalDays),
                CreatedAt = DateTime.UtcNow
            });
        }
        return true;
    }

    public async Task<bool> CompleteReminderAsync(Guid reminderId, CancellationToken cancellationToken = default)
    {
        try
        {
            var route = $"{MobileApiEndpoints.Reminders}/{reminderId}/complete";
            var response = await _apiClient.PatchAsync(route, new { }, cancellationToken);
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
            var rem = _reminders.FirstOrDefault(x => x.Id == reminderId);
            if (rem != null)
            {
                var idx = _reminders.IndexOf(rem);
                _reminders[idx] = rem with
                {
                    LastDoneAt = DateTime.UtcNow,
                    NextDueAt = DateTime.UtcNow.AddDays(rem.IntervalDays)
                };
                return true;
            }
        }
        return false;
    }

    public async Task<bool> DeleteReminderAsync(Guid reminderId, CancellationToken cancellationToken = default)
    {
        try
        {
            var route = $"{MobileApiEndpoints.Reminders}/{reminderId}";
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
            _reminders.RemoveAll(x => x.Id == reminderId);
        }
        return true;
    }
}
