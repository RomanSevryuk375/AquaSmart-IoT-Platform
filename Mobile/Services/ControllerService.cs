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
        SeedDemoControllers();
    }

    private void SeedDemoControllers()
    {
        lock (_syncLock)
        {
            _controllers.Add(new ControllerModel
            {
                Id = Guid.Parse("aaaaaaa1-aaaa-aaaa-aaaa-aaaaaaaaaaaa"),
                Name = "Контроллер Сампа №1",
                ModelType = "AquaSmart ESP32 Pro",
                MacAddress = "A4:CF:12:89:C4:22",
                IpAddress = "192.168.1.145",
                FirmwareVersion = "v2.4.1",
                IsOnline = true,
                LastSeen = DateTime.UtcNow.AddMinutes(-2),
                AttachedAquariumName = "Мой первый аквариум"
            });

            _controllers.Add(new ControllerModel
            {
                Id = Guid.Parse("aaaaaaa2-aaaa-aaaa-aaaa-aaaaaaaaaaaa"),
                Name = "Модуль освещения WRGB",
                ModelType = "AquaSmart LED Controller",
                MacAddress = "DC:4F:22:11:AB:88",
                IpAddress = "192.168.1.146",
                FirmwareVersion = "v1.8.0",
                IsOnline = true,
                LastSeen = DateTime.UtcNow.AddMinutes(-1),
                AttachedAquariumName = "Флориус Идратус"
            });
        }
    }

    public async Task<List<ControllerModel>> GetControllersAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _apiClient.GetAsync<List<ControllerDto>>(ApiConfiguration.Routes.Controllers, cancellationToken);
            if (response.IsSuccess && response.Data != null && response.Data.Count > 0)
            {
                var mapped = response.Data.Select(dto => new ControllerModel
                {
                    Id = dto.Id,
                    Name = dto.Name,
                    MacAddress = dto.MacAddress,
                    IsOnline = dto.IsOnline,
                    LastSeen = dto.LastSeenAt != default ? dto.LastSeenAt : DateTime.UtcNow,
                    ModelType = "AquaSmart ESP32 Pro",
                    IpAddress = "192.168.1.x",
                    FirmwareVersion = "v2.4.0",
                    AttachedAquariumName = "Экосистема"
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
}
