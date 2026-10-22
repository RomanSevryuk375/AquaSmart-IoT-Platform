using Mobile.Models;
using Mobile.Services.Dto.Identity;

namespace Mobile.Services;

public sealed class IdentityService : IIdentityService
{
    private const string AccessTokenKey = "aquasmart_auth_token";
    private const string RefreshTokenKey = "aquasmart_refresh_token";

    private const string SavedUserNameKey = "aquasmart_user_name";
    private const string SavedUserEmailKey = "aquasmart_user_email";
    private const string SavedUserPhoneKey = "aquasmart_user_phone";

    private readonly IApiClient _apiClient;
    private readonly object _syncLock = new();
    private UserProfileModel _currentUser = new();

    public IdentityService(IApiClient apiClient)
    {
        _apiClient = apiClient;
        LoadCachedProfile();
    }

    private void LoadCachedProfile()
    {
        lock (_syncLock)
        {
            var name = Preferences.Default.Get<string>(SavedUserNameKey, string.Empty);
            var email = Preferences.Default.Get<string>(SavedUserEmailKey, string.Empty);
            var phone = Preferences.Default.Get<string>(SavedUserPhoneKey, string.Empty);

            if (!string.IsNullOrWhiteSpace(name)) _currentUser.FullName = name;
            if (!string.IsNullOrWhiteSpace(email)) _currentUser.Email = email;
            if (!string.IsNullOrWhiteSpace(phone)) _currentUser.Phone = phone;
        }
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
            Preferences.Default.Remove(SavedUserNameKey);
            Preferences.Default.Remove(SavedUserEmailKey);
            Preferences.Default.Remove(SavedUserPhoneKey);
            lock (_syncLock)
            {
                _currentUser.FullName = string.Empty;
                _currentUser.Email = string.Empty;
                _currentUser.Phone = string.Empty;
            }
        }
        catch
        {
        }
        return Task.CompletedTask;
    }

    public async Task<UserProfileModel> GetCurrentUserAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var isAuth = await IsAuthenticatedAsync();
            if (isAuth)
            {
                var response = await _apiClient.GetAsync<UserProfileResponseDto>(ApiConfiguration.Routes.Profile, cancellationToken);
                if (response.IsSuccess && response.Data != null)
                {
                    var dto = response.Data;
                    lock (_syncLock)
                    {
                        if (!string.IsNullOrWhiteSpace(dto.Name)) _currentUser.FullName = dto.Name;
                        if (!string.IsNullOrWhiteSpace(dto.Email)) _currentUser.Email = dto.Email;
                        if (!string.IsNullOrWhiteSpace(dto.PhoneNumber)) _currentUser.Phone = dto.PhoneNumber;

                        Preferences.Default.Set(SavedUserNameKey, _currentUser.FullName);
                        Preferences.Default.Set(SavedUserEmailKey, _currentUser.Email);
                        Preferences.Default.Set(SavedUserPhoneKey, _currentUser.Phone);
                    }
                }
            }
        }
        catch
        {
        }

        lock (_syncLock)
        {
            return _currentUser;
        }
    }

    public async Task UpdateCurrentUserAsync(UserProfileModel profile, CancellationToken cancellationToken = default)
    {
        lock (_syncLock)
        {
            _currentUser = profile;
            Preferences.Default.Set(SavedUserNameKey, _currentUser.FullName);
            Preferences.Default.Set(SavedUserEmailKey, _currentUser.Email);
            Preferences.Default.Set(SavedUserPhoneKey, _currentUser.Phone);
        }

        try
        {
            var isAuth = await IsAuthenticatedAsync();
            if (isAuth)
            {
                var request = new UpdateProfileRequestDto(profile.FullName, profile.Phone);
                await _apiClient.PutAsync($"{ApiConfiguration.Routes.Profile}/me", request, cancellationToken);
            }
        }
        catch
        {
            // If offline, updated locally
        }
    }
}
