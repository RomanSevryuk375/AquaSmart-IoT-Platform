using Mobile.Services.Dto.Identity;

namespace Mobile.Services;

public class AuthService : IAuthService
{
    private readonly IIdentityService _identityService;
    private readonly IApiClient _apiClient;

    public AuthService(IIdentityService identityService, IApiClient apiClient)
    {
        _identityService = identityService;
        _apiClient = apiClient;
    }

    public async Task<(bool Success, string? ErrorMessage)> LoginAsync(string emailOrLogin, string password)
    {
        if (string.IsNullOrWhiteSpace(emailOrLogin) || string.IsNullOrWhiteSpace(password))
        {
            return (false, "Заполните все обязательные поля");
        }

        if (password.Length < 4)
        {
            return (false, "Пароль должен содержать не менее 4 символов");
        }

        try
        {
            var request = new LoginRequestDto(emailOrLogin, password);
            var response = await _apiClient.PostAsync<LoginRequestDto, LoginResponseDto>(
                ApiConfiguration.Routes.AuthLogin,
                request);

            if (response.IsSuccess && response.Data != null)
            {
                await _identityService.SetTokensAsync(response.Data.AccessToken, response.Data.RefreshToken);
                await _identityService.GetCurrentUserAsync();
                return (true, null);
            }

            // If the server explicitly rejected the credentials (400 Bad Request, 401 Unauthorized, etc.)
            if (response.StatusCode is 400 or 401 or 403 or 404)
            {
                return (false, response.ErrorMessage ?? "Неверный логин или пароль");
            }

            // If backend is unreachable (503 / 408 / connection refused), fall back to offline demo session
            await _identityService.SetTokenAsync($"offline_demo_token_{Guid.NewGuid():N}");
            var profile = await _identityService.GetCurrentUserAsync();
            if (emailOrLogin.Contains('@'))
            {
                profile.Email = emailOrLogin;
            }
            return (true, null);
        }
        catch
        {
            // Network fallback
            await _identityService.SetTokenAsync($"offline_demo_token_{Guid.NewGuid():N}");
            return (true, null);
        }
    }

    public async Task<(bool Success, string? ErrorMessage)> RegisterAsync(string name, string email, string phone, string password)
    {
        if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
        {
            return (false, "Пожалуйста, заполните обязательные поля");
        }

        if (!email.Contains('@') || !email.Contains('.'))
        {
            return (false, "Укажите корректный email адрес");
        }

        try
        {
            var request = new RegisterRequestDto(email, phone, name, password, "UTC");
            var response = await _apiClient.PostAsync<RegisterRequestDto, LoginResponseDto>(
                ApiConfiguration.Routes.AuthRegister,
                request);

            if (response.IsSuccess && response.Data != null)
            {
                await _identityService.SetTokensAsync(response.Data.AccessToken, response.Data.RefreshToken);
                var profile = await _identityService.GetCurrentUserAsync();
                profile.FullName = name;
                profile.Email = email;
                profile.Phone = phone;
                await _identityService.UpdateCurrentUserAsync(profile);
                return (true, null);
            }

            if (response.StatusCode is 400 or 409)
            {
                return (false, response.ErrorMessage ?? "Пользователь с таким email уже существует");
            }

            // Backend unreachable: fallback to offline mode
            await _identityService.SetTokenAsync($"offline_demo_token_{Guid.NewGuid():N}");
            var offlineProfile = await _identityService.GetCurrentUserAsync();
            offlineProfile.FullName = name;
            offlineProfile.Email = email;
            offlineProfile.Phone = phone;
            await _identityService.UpdateCurrentUserAsync(offlineProfile);
            return (true, null);
        }
        catch
        {
            await _identityService.SetTokenAsync($"offline_demo_token_{Guid.NewGuid():N}");
            return (true, null);
        }
    }

    public async Task<(bool Success, string? ErrorMessage)> RequestPasswordResetAsync(string emailOrPhone)
    {
        await Task.Delay(300);
        if (string.IsNullOrWhiteSpace(emailOrPhone))
        {
            return (false, "Укажите email или номер телефона");
        }
        return (true, null);
    }

    public async Task<(bool Success, string? ErrorMessage)> ResetPasswordAsync(string code, string newPassword)
    {
        await Task.Delay(300);
        if (string.IsNullOrWhiteSpace(code) || string.IsNullOrWhiteSpace(newPassword))
        {
            return (false, "Заполните код подтверждения и новый пароль");
        }
        return (true, null);
    }

    public async Task LogoutAsync()
    {
        try
        {
            await _apiClient.PostAsync(ApiConfiguration.Routes.AuthLogout, new { });
        }
        catch
        {
        }
        finally
        {
            await _identityService.ClearTokenAsync();
        }
    }
}
