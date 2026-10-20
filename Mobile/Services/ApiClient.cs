using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace Mobile.Services;

public class ApiClient : IApiClient
{
    private readonly HttpClient _httpClient;
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public ApiClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<ApiResponse<T>> GetAsync<T>(string route, CancellationToken cancellationToken = default)
    {
        try
        {
            var uri = BuildUri(route);
            using var response = await _httpClient.GetAsync(uri, cancellationToken);

            if (response.IsSuccessStatusCode)
            {
                if (response.StatusCode == HttpStatusCode.NoContent)
                {
                    return ApiResponse<T>.Success(default!);
                }

                var data = await response.Content.ReadFromJsonAsync<T>(JsonOptions, cancellationToken);
                return ApiResponse<T>.Success(data!, (int)response.StatusCode);
            }

            var error = await ParseErrorMessageAsync(response);
            return ApiResponse<T>.Failure(error, (int)response.StatusCode);
        }
        catch (TaskCanceledException)
        {
            return ApiResponse<T>.Failure("Тайм-аут подключения к серверу API Gateway", 408);
        }
        catch (HttpRequestException ex)
        {
            return ApiResponse<T>.Failure($"Ошибка сети: {ex.Message}", 503);
        }
        catch (Exception ex)
        {
            return ApiResponse<T>.Failure($"Ошибка: {ex.Message}", 500);
        }
    }

    public async Task<ApiResponse<TResponse>> PostAsync<TRequest, TResponse>(string route, TRequest data, CancellationToken cancellationToken = default)
    {
        try
        {
            var uri = BuildUri(route);
            using var response = await _httpClient.PostAsJsonAsync(uri, data, JsonOptions, cancellationToken);

            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<TResponse>(JsonOptions, cancellationToken);
                return ApiResponse<TResponse>.Success(result!, (int)response.StatusCode);
            }

            var error = await ParseErrorMessageAsync(response);
            return ApiResponse<TResponse>.Failure(error, (int)response.StatusCode);
        }
        catch (TaskCanceledException)
        {
            return ApiResponse<TResponse>.Failure("Тайм-аут подключения к серверу API Gateway", 408);
        }
        catch (HttpRequestException ex)
        {
            return ApiResponse<TResponse>.Failure($"Ошибка сети: {ex.Message}", 503);
        }
        catch (Exception ex)
        {
            return ApiResponse<TResponse>.Failure($"Ошибка: {ex.Message}", 500);
        }
    }

    public async Task<ApiResponse> PostAsync<TRequest>(string route, TRequest data, CancellationToken cancellationToken = default)
    {
        try
        {
            var uri = BuildUri(route);
            using var response = await _httpClient.PostAsJsonAsync(uri, data, JsonOptions, cancellationToken);

            if (response.IsSuccessStatusCode)
            {
                return ApiResponse.Success((int)response.StatusCode);
            }

            var error = await ParseErrorMessageAsync(response);
            return ApiResponse.Failure(error, (int)response.StatusCode);
        }
        catch (TaskCanceledException)
        {
            return ApiResponse.Failure("Тайм-аут подключения к серверу API Gateway", 408);
        }
        catch (HttpRequestException ex)
        {
            return ApiResponse.Failure($"Ошибка сети: {ex.Message}", 503);
        }
        catch (Exception ex)
        {
            return ApiResponse.Failure($"Ошибка: {ex.Message}", 500);
        }
    }

    public async Task<ApiResponse> PutAsync<TRequest>(string route, TRequest data, CancellationToken cancellationToken = default)
    {
        try
        {
            var uri = BuildUri(route);
            using var response = await _httpClient.PutAsJsonAsync(uri, data, JsonOptions, cancellationToken);

            if (response.IsSuccessStatusCode)
            {
                return ApiResponse.Success((int)response.StatusCode);
            }

            var error = await ParseErrorMessageAsync(response);
            return ApiResponse.Failure(error, (int)response.StatusCode);
        }
        catch (TaskCanceledException)
        {
            return ApiResponse.Failure("Тайм-аут подключения к серверу API Gateway", 408);
        }
        catch (HttpRequestException ex)
        {
            return ApiResponse.Failure($"Ошибка сети: {ex.Message}", 503);
        }
        catch (Exception ex)
        {
            return ApiResponse.Failure($"Ошибка: {ex.Message}", 500);
        }
    }

    public async Task<ApiResponse> DeleteAsync(string route, CancellationToken cancellationToken = default)
    {
        try
        {
            var uri = BuildUri(route);
            using var response = await _httpClient.DeleteAsync(uri, cancellationToken);

            if (response.IsSuccessStatusCode)
            {
                return ApiResponse.Success((int)response.StatusCode);
            }

            var error = await ParseErrorMessageAsync(response);
            return ApiResponse.Failure(error, (int)response.StatusCode);
        }
        catch (TaskCanceledException)
        {
            return ApiResponse.Failure("Тайм-аут подключения к серверу API Gateway", 408);
        }
        catch (HttpRequestException ex)
        {
            return ApiResponse.Failure($"Ошибка сети: {ex.Message}", 503);
        }
        catch (Exception ex)
        {
            return ApiResponse.Failure($"Ошибка: {ex.Message}", 500);
        }
    }

    private static string BuildUri(string route)
    {
        if (route.StartsWith("http://", StringComparison.OrdinalIgnoreCase) ||
            route.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
        {
            return route;
        }

        var baseUri = ApiConfiguration.BaseUrl;
        return $"{baseUri}{route.TrimStart('/')}";
    }

    private static async Task<string> ParseErrorMessageAsync(HttpResponseMessage response)
    {
        try
        {
            var content = await response.Content.ReadAsStringAsync();
            if (string.IsNullOrWhiteSpace(content))
            {
                return $"Запрос завершился с кодом {(int)response.StatusCode} ({response.ReasonPhrase})";
            }

            using var doc = JsonDocument.Parse(content);
            var root = doc.RootElement;
            if (root.TryGetProperty("detail", out var detail) && !string.IsNullOrWhiteSpace(detail.GetString()))
            {
                return detail.GetString()!;
            }
            if (root.TryGetProperty("message", out var message) && !string.IsNullOrWhiteSpace(message.GetString()))
            {
                return message.GetString()!;
            }
            if (root.TryGetProperty("title", out var title) && !string.IsNullOrWhiteSpace(title.GetString()))
            {
                return title.GetString()!;
            }

            return content;
        }
        catch
        {
            return $"Запрос завершился с кодом {(int)response.StatusCode}";
        }
    }
}
