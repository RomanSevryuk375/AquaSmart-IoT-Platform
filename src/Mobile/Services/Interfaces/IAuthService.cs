namespace Mobile.Services.Interfaces;

public interface IAuthService
{
    Task<(bool Success, string? ErrorMessage)> LoginAsync(string emailOrLogin, string password, CancellationToken cancellationToken = default);
    Task<(bool Success, string? ErrorMessage)> RegisterAsync(string name, string email, string phone, string password, CancellationToken cancellationToken = default);
    Task<(bool Success, string? ErrorMessage)> RequestPasswordResetAsync(string emailOrPhone, CancellationToken cancellationToken = default);
    Task<(bool Success, string? ErrorMessage)> ResetPasswordAsync(string code, string newPassword, CancellationToken cancellationToken = default);
    Task<bool> TryAutoLoginAsync(CancellationToken cancellationToken = default);
    Task LogoutAsync(CancellationToken cancellationToken = default);
}
