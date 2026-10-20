namespace Mobile.Services.Interfaces;

public interface IControllerService
{
    Task<List<ControllerModel>> GetControllersAsync();
    Task<ControllerModel?> GetControllerByIdAsync(Guid id);
    Task AddControllerAsync(ControllerModel controller);
    Task<bool> DeleteControllerAsync(Guid id);
    Task<bool> RebootControllerAsync(Guid id);
}
