namespace Mobile.Services.Interfaces;

public interface IAuthService
{
    Task<(bool Success, string? ErrorMessage)> LoginAsync(string emailOrLogin, string password);
    Task<(bool Success, string? ErrorMessage)> RegisterAsync(string name, string email, string phone, string password);
    Task<(bool Success, string? ErrorMessage)> RequestPasswordResetAsync(string emailOrPhone);
    Task<(bool Success, string? ErrorMessage)> ResetPasswordAsync(string code, string newPassword);
    Task LogoutAsync();
}
