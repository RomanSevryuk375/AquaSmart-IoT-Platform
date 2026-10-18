using Mobile.Models;

namespace Mobile.Services;

public interface IIdentityService
{
    Task<bool> IsAuthenticatedAsync();
    Task<string?> GetTokenAsync();
    Task SetTokenAsync(string token);
    Task ClearTokenAsync();
    Task<UserProfileModel> GetCurrentUserAsync();
    Task UpdateCurrentUserAsync(UserProfileModel profile);
}

public class IdentityService : IIdentityService
{
    private const string TokenKey = "aquasmart_auth_token";
    private UserProfileModel _currentUser = new();

    public async Task<bool> IsAuthenticatedAsync()
    {
        var token = await GetTokenAsync();
        return !string.IsNullOrWhiteSpace(token);
    }

    public async Task<string?> GetTokenAsync()
    {
        try
        {
            return await SecureStorage.Default.GetAsync(TokenKey);
        }
        catch
        {
            return null;
        }
    }

    public async Task SetTokenAsync(string token)
    {
        try
        {
            await SecureStorage.Default.SetAsync(TokenKey, token);
        }
        catch
        {
        }
    }

    public Task ClearTokenAsync()
    {
        try
        {
            SecureStorage.Default.Remove(TokenKey);
        }
        catch
        {
        }
        return Task.CompletedTask;
    }

    public Task<UserProfileModel> GetCurrentUserAsync()
    {
        return Task.FromResult(_currentUser);
    }

    public Task UpdateCurrentUserAsync(UserProfileModel profile)
    {
        _currentUser = profile;
        return Task.CompletedTask;
    }
}
