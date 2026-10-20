namespace Mobile.Services.Interfaces;

public interface IIdentityService
{
    Task<bool> IsAuthenticatedAsync();
    Task<string?> GetTokenAsync();
    Task<string?> GetRefreshTokenAsync();
    Task SetTokensAsync(string accessToken, string refreshToken);
    Task SetTokenAsync(string token);
    Task ClearTokenAsync();
    Task<UserProfileModel> GetCurrentUserAsync();
    Task UpdateCurrentUserAsync(UserProfileModel profile);
}
