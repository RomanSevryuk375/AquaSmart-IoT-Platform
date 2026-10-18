namespace Mobile.Services;

public interface IAuthService
{
    Task<(bool Success, string? ErrorMessage)> LoginAsync(string emailOrLogin, string password);
    Task<(bool Success, string? ErrorMessage)> RegisterAsync(string name, string email, string phone, string password);
    Task<(bool Success, string? ErrorMessage)> RequestPasswordResetAsync(string emailOrPhone);
    Task<(bool Success, string? ErrorMessage)> ResetPasswordAsync(string code, string newPassword);
    Task LogoutAsync();
}

public class AuthService : IAuthService
{
    private readonly IIdentityService _identityService;

    public AuthService(IIdentityService identityService)
    {
        _identityService = identityService;
    }

    public async Task<(bool Success, string? ErrorMessage)> LoginAsync(string emailOrLogin, string password)
    {
        // Simulated network delay
        await Task.Delay(500);

        if (string.IsNullOrWhiteSpace(emailOrLogin) || string.IsNullOrWhiteSpace(password))
        {
            return (false, "Заполните все обязательные поля");
        }

        if (password.Length < 4)
        {
            return (false, "Пароль должен содержать не менее 4 символов");
        }

        // Generate demo session token
        await _identityService.SetTokenAsync($"demo_jwt_token_{Guid.NewGuid():N}");
        return (true, null);
    }

    public async Task<(bool Success, string? ErrorMessage)> RegisterAsync(string name, string email, string phone, string password)
    {
        await Task.Delay(600);

        if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
        {
            return (false, "Пожалуйста, заполните обязательные поля");
        }

        if (!email.Contains('@') || !email.Contains('.'))
        {
            return (false, "Укажите корректный email адрес");
        }

        await _identityService.SetTokenAsync($"demo_jwt_token_{Guid.NewGuid():N}");
        var profile = await _identityService.GetCurrentUserAsync();
        profile.FullName = name;
        profile.Email = email;
        profile.Phone = phone;
        await _identityService.UpdateCurrentUserAsync(profile);

        return (true, null);
    }

    public async Task<(bool Success, string? ErrorMessage)> RequestPasswordResetAsync(string emailOrPhone)
    {
        await Task.Delay(500);
        if (string.IsNullOrWhiteSpace(emailOrPhone))
        {
            return (false, "Укажите email или номер телефона");
        }
        return (true, null);
    }

    public async Task<(bool Success, string? ErrorMessage)> ResetPasswordAsync(string code, string newPassword)
    {
        await Task.Delay(500);
        if (string.IsNullOrWhiteSpace(code) || string.IsNullOrWhiteSpace(newPassword))
        {
            return (false, "Заполните код подтверждения и новый пароль");
        }
        return (true, null);
    }

    public async Task LogoutAsync()
    {
        await _identityService.ClearTokenAsync();
    }
}
