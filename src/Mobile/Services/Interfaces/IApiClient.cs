namespace Mobile.Services.Interfaces;

public interface IApiClient
{
    Task<ApiResponse<T>> GetAsync<T>(string route, CancellationToken cancellationToken = default);
    Task<ApiResponse<TResponse>> PostAsync<TRequest, TResponse>(string route, TRequest data, CancellationToken cancellationToken = default);
    Task<ApiResponse> PostAsync<TRequest>(string route, TRequest data, CancellationToken cancellationToken = default);
    Task<ApiResponse> PutAsync<TRequest>(string route, TRequest data, CancellationToken cancellationToken = default);
    Task<ApiResponse> PatchAsync<TRequest>(string route, TRequest data, CancellationToken cancellationToken = default);
    Task<ApiResponse> DeleteAsync(string route, CancellationToken cancellationToken = default);
}
