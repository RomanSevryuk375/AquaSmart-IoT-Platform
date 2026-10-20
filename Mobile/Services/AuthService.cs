using Mobile.Services.Dto.Identity;

namespace Mobile.Services;

public sealed class AuthService : IAuthService
{
    private readonly IIdentityService _identityService;
    private readonly IApiClient _apiClient;

    public AuthService(IIdentityService identityService, IApiClient apiClient)
    {
        _identityService = identityService;
        _apiClient = apiClient;
    }

    public async Task<(bool Success, string? ErrorMessage)> LoginAsync(string emailOrLogin, string password, CancellationToken cancellationToken = default)
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
                request,
                cancellationToken);

            if (response.IsSuccess && response.Data != null)
            {
                await _identityService.SetTokensAsync(response.Data.AccessToken, response.Data.RefreshToken);
                await _identityService.GetCurrentUserAsync(cancellationToken);
                return (true, null);
            }

            if (response.StatusCode is 400 or 401 or 403 or 404)
            {
                return (false, response.ErrorMessage ?? "Неверный логин или пароль");
            }

            // Offline fallback
            await _identityService.SetTokenAsync($"offline_demo_token_{Guid.NewGuid():N}");
            var profile = await _identityService.GetCurrentUserAsync(cancellationToken);
            if (emailOrLogin.Contains('@'))
            {
                profile.Email = emailOrLogin;
            }
            return (true, null);
        }
        catch
        {
            await _identityService.SetTokenAsync($"offline_demo_token_{Guid.NewGuid():N}");
            return (true, null);
        }
    }

    public async Task<(bool Success, string? ErrorMessage)> RegisterAsync(string name, string email, string phone, string password, CancellationToken cancellationToken = default)
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
                request,
                cancellationToken);

            if (response.IsSuccess && response.Data != null)
            {
                await _identityService.SetTokensAsync(response.Data.AccessToken, response.Data.RefreshToken);
                var profile = await _identityService.GetCurrentUserAsync(cancellationToken);
                profile.FullName = name;
                profile.Email = email;
                profile.Phone = phone;
                await _identityService.UpdateCurrentUserAsync(profile, cancellationToken);
                return (true, null);
            }

            if (response.StatusCode is 400 or 409)
            {
                return (false, response.ErrorMessage ?? "Пользователь с таким email уже существует");
            }

            await _identityService.SetTokenAsync($"offline_demo_token_{Guid.NewGuid():N}");
            var offlineProfile = await _identityService.GetCurrentUserAsync(cancellationToken);
            offlineProfile.FullName = name;
            offlineProfile.Email = email;
            offlineProfile.Phone = phone;
            await _identityService.UpdateCurrentUserAsync(offlineProfile, cancellationToken);
            return (true, null);
        }
        catch
        {
            await _identityService.SetTokenAsync($"offline_demo_token_{Guid.NewGuid():N}");
            return (true, null);
        }
    }

    public async Task<(bool Success, string? ErrorMessage)> RequestPasswordResetAsync(string emailOrPhone, CancellationToken cancellationToken = default)
    {
        await Task.Delay(200, cancellationToken);
        if (string.IsNullOrWhiteSpace(emailOrPhone))
        {
            return (false, "Укажите email или номер телефона");
        }
        return (true, null);
    }

    public async Task<(bool Success, string? ErrorMessage)> ResetPasswordAsync(string code, string newPassword, CancellationToken cancellationToken = default)
    {
        await Task.Delay(200, cancellationToken);
        if (string.IsNullOrWhiteSpace(code) || string.IsNullOrWhiteSpace(newPassword))
        {
            return (false, "Заполните код подтверждения и новый пароль");
        }
        return (true, null);
    }

    public async Task LogoutAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            await _apiClient.PostAsync(ApiConfiguration.Routes.AuthLogout, new { }, cancellationToken);
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
