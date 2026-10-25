namespace Mobile.Services.Interfaces;

public interface ISecurityService
{
    Task<bool> CheckRelayPinAsync(string relayName);
    Task<bool> CheckAppUnlockPinAsync();
    bool ShouldAutoLock(DateTime lastActiveTime);
    Task<bool> CanUseBiometricsOrPinAsync();
    Task<bool> AuthenticateBiometricOrPinAsync();
}
