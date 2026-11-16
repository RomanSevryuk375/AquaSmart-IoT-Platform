using Mobile.Models;
using Mobile.Services.Dto.Device;

namespace Mobile.Services;

public sealed class ControllerService : IControllerService
{
    private readonly IApiClient _apiClient;
    private readonly object _syncLock = new();
    private readonly List<ControllerModel> _controllers = new();

    public ControllerService(IApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    public async Task<List<ControllerModel>> GetControllersAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _apiClient.GetAsync<List<ControllerDto>>(ApiConfiguration.Routes.Controllers, cancellationToken);
            if (response.IsSuccess && response.Data != null)
            {
                var mapped = response.Data.Select(dto => new ControllerModel
                {
                    Id = dto.Id,
                    Name = dto.Name,
                    MacAddress = dto.MacAddress,
                    IsOnline = dto.IsOnline,
                    LastSeen = dto.LastSeenAt != default ? dto.LastSeenAt : DateTime.UtcNow,
                    ModelType = "AquaSmart ESP32",
                    IpAddress = "—",
                    FirmwareVersion = "—",
                    AttachedAquariumName = "Не привязан"
                }).ToList();

                lock (_syncLock)
                {
                    _controllers.Clear();
                    _controllers.AddRange(mapped);
                }
                return mapped;
            }
        }
        catch
        {
        }

        lock (_syncLock)
        {
            return _controllers.ToList();
        }
    }

    public async Task<ControllerModel?> GetControllerByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _apiClient.GetAsync<ControllerDto>($"{ApiConfiguration.Routes.Controllers}/{id}", cancellationToken);
            if (response.IsSuccess && response.Data != null)
            {
                var dto = response.Data;
                lock (_syncLock)
                {
                    var found = _controllers.FirstOrDefault(c => c.Id == id);
                    if (found != null)
                    {
                        found.Name = dto.Name;
                        found.IsOnline = dto.IsOnline;
                        return found;
                    }
                }
            }
        }
        catch
        {
        }

        lock (_syncLock)
        {
            return _controllers.FirstOrDefault(c => c.Id == id);
        }
    }

    public async Task AddControllerAsync(ControllerModel controller, CancellationToken cancellationToken = default)
    {
        try
        {
            var request = new AddControllerRequestDto(
                controller.MacAddress,
                controller.Name,
                controller.IsOnline);

            await _apiClient.PostAsync(ApiConfiguration.Routes.Controllers, request, cancellationToken);
        }
        catch
        {
        }

        lock (_syncLock)
        {
            _controllers.Add(controller);
        }
    }

    public async Task<bool> DeleteControllerAsync(Guid id, CancellationToken cancellationToken = default)
    {
        try
        {
            await _apiClient.DeleteAsync($"{ApiConfiguration.Routes.Controllers}/{id}", cancellationToken);
        }
        catch
        {
        }

        lock (_syncLock)
        {
            var count = _controllers.RemoveAll(c => c.Id == id);
            return count > 0;
        }
    }

    public Task<bool> RebootControllerAsync(Guid id, CancellationToken cancellationToken = default)
    {
        lock (_syncLock)
        {
            var c = _controllers.FirstOrDefault(x => x.Id == id);
            if (c != null)
            {
                c.LastSeen = DateTime.UtcNow;
                return Task.FromResult(true);
            }
        }
        return Task.FromResult(false);
    }

    public async Task<OtaCheckResponseDto?> CheckOtaUpdateAsync(Guid controllerId, CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _apiClient.GetAsync<OtaCheckResponseDto>(
                $"{MobileApiEndpoints.FirmwareOtaCheck}?controller_id={controllerId}",
                cancellationToken);

            if (response.IsSuccess && response.Data != null)
            {
                return response.Data;
            }
        }
        catch
        {
        }

        return null;
    }
}
