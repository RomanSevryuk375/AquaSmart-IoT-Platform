using Mobile.Models;
using Mobile.Services.Dto.Identity;

namespace Mobile.Services;

public class IdentityService : IIdentityService
{
    private const string AccessTokenKey = "aquasmart_auth_token";
    private const string RefreshTokenKey = "aquasmart_refresh_token";

    private readonly IApiClient _apiClient;
    private UserProfileModel _currentUser = new()
    {
        FullName = "Алексей Смирнов",
        Email = "alex.smirnov@aquasmart.io",
        Phone = "+7 (999) 123-45-67",
        Role = "Владелец аквасистемы",
        PushNotificationsEnabled = true,
        CriticalAlertsOnly = false,
        DarkThemeEnabled = false
    };

    public IdentityService(IApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    public async Task<bool> IsAuthenticatedAsync()
    {
        var token = await GetTokenAsync();
        return !string.IsNullOrWhiteSpace(token);
    }

    public async Task<string?> GetTokenAsync()
    {
        try
        {
            return await SecureStorage.Default.GetAsync(AccessTokenKey);
        }
        catch
        {
            return null;
        }
    }

    public async Task<string?> GetRefreshTokenAsync()
    {
        try
        {
            return await SecureStorage.Default.GetAsync(RefreshTokenKey);
        }
        catch
        {
            return null;
        }
    }

    public async Task SetTokensAsync(string accessToken, string refreshToken)
    {
        try
        {
            await SecureStorage.Default.SetAsync(AccessTokenKey, accessToken);
            if (!string.IsNullOrWhiteSpace(refreshToken))
            {
                await SecureStorage.Default.SetAsync(RefreshTokenKey, refreshToken);
            }
        }
        catch
        {
        }
    }

    public async Task SetTokenAsync(string token)
    {
        try
        {
            await SecureStorage.Default.SetAsync(AccessTokenKey, token);
        }
        catch
        {
        }
    }

    public Task ClearTokenAsync()
    {
        try
        {
            SecureStorage.Default.Remove(AccessTokenKey);
            SecureStorage.Default.Remove(RefreshTokenKey);
        }
        catch
        {
        }
        return Task.CompletedTask;
    }

    public async Task<UserProfileModel> GetCurrentUserAsync()
    {
        try
        {
            var isAuth = await IsAuthenticatedAsync();
            if (isAuth)
            {
                var response = await _apiClient.GetAsync<UserProfileResponseDto>(ApiConfiguration.Routes.Profile);
                if (response.IsSuccess && response.Data != null)
                {
                    var dto = response.Data;
                    _currentUser.FullName = string.IsNullOrWhiteSpace(dto.Name) ? _currentUser.FullName : dto.Name;
                    _currentUser.Email = string.IsNullOrWhiteSpace(dto.Email) ? _currentUser.Email : dto.Email;
                    if (!string.IsNullOrWhiteSpace(dto.PhoneNumber))
                    {
                        _currentUser.Phone = dto.PhoneNumber;
                    }
                }
            }
        }
        catch
        {
            // Offline or backend not reachable: silently fallback to local cached profile
        }

        return _currentUser;
    }

    public async Task UpdateCurrentUserAsync(UserProfileModel profile)
    {
        _currentUser = profile;

        try
        {
            var isAuth = await IsAuthenticatedAsync();
            if (isAuth)
            {
                var request = new UpdateProfileRequestDto(profile.FullName, profile.Phone);
                await _apiClient.PutAsync(ApiConfiguration.Routes.Profile, request);
            }
        }
        catch
        {
            // If offline, updated locally
        }
    }
}
