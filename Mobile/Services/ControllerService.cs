using Mobile.Models;

namespace Mobile.Services;

public interface IControllerService
{
    Task<List<ControllerModel>> GetControllersAsync();
    Task<ControllerModel?> GetControllerByIdAsync(Guid id);
    Task AddControllerAsync(ControllerModel controller);
    Task<bool> DeleteControllerAsync(Guid id);
    Task<bool> RebootControllerAsync(Guid id);
}

public class ControllerService : IControllerService
{
    private readonly List<ControllerModel> _controllers = new();

    public ControllerService()
    {
        _controllers.Add(new ControllerModel
        {
            Id = Guid.NewGuid(),
            Name = "Контроллер Саммпа №1",
            ModelType = "AquaSmart ESP32 Pro",
            MacAddress = "A4:CF:12:89:C4:22",
            IpAddress = "192.168.1.145",
            FirmwareVersion = "v2.4.1",
            IsOnline = true,
            LastSeen = DateTime.UtcNow.AddMinutes(-2),
            AttachedAquariumName = "Биотоп Танганьика №1"
        });

        _controllers.Add(new ControllerModel
        {
            Id = Guid.NewGuid(),
            Name = "Модуль освещения WRGB",
            ModelType = "AquaSmart LED Controller",
            MacAddress = "DC:4F:22:11:AB:88",
            IpAddress = "192.168.1.146",
            FirmwareVersion = "v1.8.0",
            IsOnline = true,
            LastSeen = DateTime.UtcNow.AddMinutes(-1),
            AttachedAquariumName = "Голландский травник"
        });
    }

    public Task<List<ControllerModel>> GetControllersAsync()
    {
        return Task.FromResult(_controllers.ToList());
    }

    public Task<ControllerModel?> GetControllerByIdAsync(Guid id)
    {
        var found = _controllers.FirstOrDefault(c => c.Id == id);
        return Task.FromResult(found);
    }

    public Task AddControllerAsync(ControllerModel controller)
    {
        _controllers.Add(controller);
        return Task.CompletedTask;
    }

    public Task<bool> DeleteControllerAsync(Guid id)
    {
        var count = _controllers.RemoveAll(c => c.Id == id);
        return Task.FromResult(count > 0);
    }

    public Task<bool> RebootControllerAsync(Guid id)
    {
        var c = _controllers.FirstOrDefault(x => x.Id == id);
        if (c != null)
        {
            c.LastSeen = DateTime.UtcNow;
            return Task.FromResult(true);
        }
        return Task.FromResult(false);
    }
}
