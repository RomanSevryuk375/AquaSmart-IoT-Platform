using System.Net.Http.Headers;

namespace Mobile.Services;

public sealed class AuthHeaderHandler : DelegatingHandler
{
    private const string TokenKey = "aquasmart_auth_token";

    public AuthHeaderHandler()
    {
    }

    public AuthHeaderHandler(HttpMessageHandler innerHandler) : base(innerHandler)
    {
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        try
        {
            var token = await SecureStorage.Default.GetAsync(TokenKey);
            if (!string.IsNullOrWhiteSpace(token))
            {
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }
        }
        catch
        {
            // SecureStorage access might fail on some platforms if not initialized; silently proceed without crashing
        }

        if (!request.Headers.Accept.Any(h => h.MediaType == "application/json"))
        {
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        }

        return await base.SendAsync(request, cancellationToken);
    }
}
