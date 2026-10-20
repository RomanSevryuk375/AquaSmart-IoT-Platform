namespace Mobile.Services.Interfaces;

public interface IControllerService
{
    Task<List<ControllerModel>> GetControllersAsync(CancellationToken cancellationToken = default);
    Task<ControllerModel?> GetControllerByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task AddControllerAsync(ControllerModel controller, CancellationToken cancellationToken = default);
    Task<bool> DeleteControllerAsync(Guid id, CancellationToken cancellationToken = default);
    Task<bool> RebootControllerAsync(Guid id, CancellationToken cancellationToken = default);
}
